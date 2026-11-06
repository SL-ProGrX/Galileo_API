using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public partial class FrmFndContratosDB
    {

        private const string SqlValidaEstados = "EXEC dbo.spFND_ValidaEstados @CodPlan, @Operadora, @Cedula;";
        private const string SpInversionTasasCondiciones = "spFnd_Inversion_Tasas_Condiciones";
        private const string SpTrdDocumentosIns = "spTrdDocumentosIns";

        private const string SqlPlanContratosActivos = @"
                    SELECT num_contratos_activos
                    FROM dbo.fnd_planes
                    WHERE cod_operadora = @Operadora
                      AND cod_plan = @CodPlan;";

        private const string SqlContratosActivosPersona = @"
                    SELECT ISNULL(COUNT(1), 0)
                    FROM dbo.fnd_contratos
                    WHERE cod_operadora = @Operadora
                      AND estado = 'A'
                      AND cedula = @Cedula
                      AND cod_plan = @CodPlan;";

        private const string SqlValidacionesContrato = @"
                    SELECT dbo.fxFnd_Contrato_Valida_Plazo(@Operadora, @CodPlan, @Plazo) AS Plazo_Valida,
                           dbo.fxFnd_Seguridad_Acceso_Planes(@Usuario, @Operadora, @CodPlan) AS Acceso_Valida,
                           (SELECT COUNT(1)
                            FROM dbo.FND_PLANES_DESTINOS_AHORRO
                            WHERE cod_Plan = @CodPlan
                              AND activo = 1) AS Destinos;";

        private const string SqlMontosMinimosPlan = @"
                    SELECT PLAZO_MINIMO * CASE WHEN PLAZO_TIPO = 'M' THEN 30 ELSE 1 END AS Plazo_Minimo,
                           MONTO_MINIMO,
                           INVERSION_MINIMO,
                           CUENTA_MAESTRA,
                           TIPO_CDP,
                           WEB_VENCE
                    FROM dbo.fnd_Planes
                    WHERE cod_operadora = @Operadora
                      AND cod_plan = @CodPlan;";

        // Paridad VB6 (sbGuardar): un solo SP registra o modifica según el código del contrato (0 = nuevo)
        private const string SpContratoRegistro = "spFnd_Contrato_Registro";

        private const string SqlContratoRegistro =
            "exec " + SpContratoRegistro +
            " @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10, @p11, @p12, @p13, @p14, @p15, @p16, @p17" +
            ", @p18, @p19, @p20, @p21, @p22, @p23, @p24, @p25, @p26, @p27, @p28, @p29, @p30, @p31, @p32, @p33";

        private const string SqlRequiereBeneficiarios = @"
                    SELECT REQUIERE_BENEFICIARIOS
                    FROM dbo.fnd_planes
                    WHERE cod_plan = @CodPlan
                      AND cod_operadora = @Operadora;";

        private const string SqlTasaRefContrato = @"
                    SELECT dbo.fxFNDCalcularTasaRefContrato(@Operadora, @Plan, @Plazo, @Tipo, NULL, NULL, 0);";

        private const string SqlInsertContratoCambio = @"
                    INSERT INTO dbo.fnd_contratos_cambios
                    (
                        cod_operadora,
                        cod_plan,
                        cod_contrato,
                        usuario,
                        fecha,
                        movimiento,
                        detalle
                    )
                    VALUES
                    (
                        @Operadora,
                        @CodPlan,
                        @Contrato,
                        @Usuario,
                        dbo.MyGetdate(),
                        @Movimiento,
                        @Detalle
                    );";

        private const string SqlTrazabilidadConsecutivo = @"
                    SELECT COUNT(1) + 1
                    FROM dbo.TrdDocumentos
                    WHERE CodDocumento = '04'
                      AND Consecutivo LIKE @Consecutivo;";

        private const string SqlBeneficiariosPorcentaje = @"
                    SELECT ISNULL(SUM(porcentaje), 0)
                    FROM dbo.FND_CONTRATOS_BENEFICIARIOS
                    WHERE cod_plan = @CodPlan
                      AND cod_operadora = @Operadora
                      AND cod_contrato = @Contrato;";

        #region funciones privadas
        /// <summary>
        /// Valida reglas de negocio requeridas antes de guardar un contrato.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="contrato">Datos del contrato a validar.</param>
        /// <returns>Resultado de la validación.</returns>
        private ErrorDto fxVerificaDatos(int CodEmpresa, ContratosModels contrato)
        {
            var response = DbHelper.CreateOkResponse();

            try
            {
                if (contrato is null)
                {
                    return DbHelper.ErrorResponse("Los datos del contrato son requeridos.", -2);
                }

                var mensajes = new List<string>();
                ValidarCantidadContratosActivos(CodEmpresa, contrato, mensajes);
                ValidarEstadoPersonaPlan(CodEmpresa, contrato, mensajes);
                ValidarReglasPlan(CodEmpresa, contrato, mensajes);
                ValidarCamposContrato(contrato, mensajes);
                ValidarMontosMinimos(CodEmpresa, contrato, mensajes);

                if (mensajes.Count > 0)
                {
                    response.Code = -1;
                    response.Description = string.Join(string.Empty, mensajes);
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Registra o modifica un contrato con spFnd_Contrato_Registro (paridad VB6: sbGuardar).
        /// El SP decide por el código del contrato (0 = nuevo) y se encarga del consecutivo, la tasa,
        /// el historial de cambios, las etiquetas de seguimiento y los cupones CDP.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="usuario">Usuario que registra o modifica el contrato.</param>
        /// <param name="contrato">Datos del contrato.</param>
        /// <returns>Resultado de la operación; el código final queda en contrato.cod_contrato.</returns>
        private ErrorDto registrarContrato(int CodEmpresa, string usuario, ContratosModels contrato)
        {
            var response = DbHelper.CreateOkResponse();

            try
            {
                var codigo = contrato.isNew ? 0 : contrato.cod_contrato;

                var registro = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                    connection.QueryFirstOrDefault<FndContratoRegistroData>(
                        SqlContratoRegistro,
                        CrearParametrosContratoRegistro(contrato, codigo, usuario)));

                if (registro.Code != 0 || registro.Result is null)
                {
                    return DbHelper.ErrorResponse(
                        registro.Description ?? "No se recibió respuesta al registrar el contrato.",
                        registro.Code.GetValueOrDefault(-1));
                }

                contrato.cod_contrato = registro.Result.cod_contrato;

                // VB6 compara Movimiento = "Registra"; el SP devuelve el código '05' para el registro inicial
                var esNuevo = registro.Result.movimiento is "05" or "Registra";

                RegistrarBitacoraContrato(CodEmpresa, usuario, contrato, esNuevo ? "Registra - WEB" : "Modifica - WEB");
                sbTrazabilidad_Inserta(CodEmpresa, "04", CrearConsecutivoTrazabilidad(contrato), contrato.cod_contrato.ToString(), usuario, esNuevo);

                // Paridad VB6: guarda Destinos/Objetivos elegidos antes de registrar el contrato
                GuardarDestinosPendientes(CodEmpresa, usuario, contrato);

                if (esNuevo)
                {
                    ValidarBeneficiariosContrato(CodEmpresa, contrato, response);
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        private ErrorDto<bool> fxAplicaBeneficiarios(int CodEmpresa, string plan, int operadora)
        {
            var result = DbHelper.ExecuteSingleQuery(
                CreatePortalDb(),
                CodEmpresa,
                SqlRequiereBeneficiarios,
                false,
                new
                {
                    CodPlan = NormalizarTexto(plan),
                    Operadora = operadora
                });

            return new ErrorDto<bool>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result
            };
        }

        /// <summary>
        /// Calcula la tasa de referencia aplicable a un contrato según plan, plazo y cupón.
        /// </summary>
        /// <param name="param">Parámetros requeridos para calcular la tasa de referencia.</param>
        /// <returns>Tasa de referencia calculada.</returns>
        public ErrorDto<decimal> fxTasaRef(FndContratoTasaRefParams param)
        {
            try
            {
                if (param is null)
                {
                    return DbHelper.CreateErrorResponse("Los parámetros para calcular la tasa son requeridos.", -2, 0m);
                }
                if (DebeUsarTasaCalculada(param.ChkCuponPaga, param.TipoCdp))
                {
                    return DbHelper.ExecuteSingleQuery(
                        CreatePortalDb(),
                        param.CodEmpresa,
                        SqlTasaRefContrato,
                        0m,
                        new
                        {
                            Operadora = param.Operadora,
                            Plan = NormalizarTexto(param.Plan),
                            Plazo = param.Plazo,
                            Tipo = NormalizarTexto(param.Tipo)
                        });
                }

                if (!string.IsNullOrWhiteSpace(param.CuponFrecuencia))
                {
                    return DbHelper.CreateOkResponse(param.Tasa > 0 ? param.Tasa : 0m);
                }

                var result = DbHelper.WithConn(CreatePortalDb(), param.CodEmpresa, connection =>
                    connection.QueryFirstOrDefault<decimal>(
                        SpInversionTasasCondiciones,
                        new
                        {
                            Operadora = param.Operadora,
                            Plan = NormalizarTexto(param.Plan),
                            PlazoId = NormalizarTexto(param.PlazoInversion),
                            fCuponId = NormalizarTexto(param.CuponFrecuencia)
                        },
                        commandType: System.Data.CommandType.StoredProcedure));

                return new ErrorDto<decimal>
                {
                    Code = result.Code,
                    Description = result.Description,
                    Result = result.Result
                };
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, 0m);
            }
        }


        private void sbGuardaCambios(int CodEmpresa, int operadora, string plan, long contrato, string usuario, int movimiento, string detalle)
        {
            try
            {
                DbHelper.ExecuteNonQuery(
                    CreatePortalDb(),
                    CodEmpresa,
                    SqlInsertContratoCambio,
                    new
                    {
                        Operadora = operadora,
                        CodPlan = NormalizarTexto(plan),
                        Contrato = contrato,
                        Usuario = NormalizarTexto(usuario),
                        Movimiento = movimiento,
                        Detalle = NormalizarTexto(detalle)
                    });
            }
            catch
            {
                // Se conserva comportamiento histórico: los errores de bitácora de cambios no interrumpen la operación principal.
            }
        }

        private void sbTrazabilidad_Inserta(int CodEmpresa, string CodDocumento, string Consecutivo, string CodBarras, string usuario, bool Nuevo = true)
        {
            try
            {
                if (CodEmpresa != 61)
                {
                    return;
                }

                var consecutivoSeguro = NormalizarTexto(Consecutivo);
                if (CodDocumento == "04" && !Nuevo)
                {
                    var consec = DbHelper.ExecuteSingleQuery(
                        CreatePortalDb(),
                        CodEmpresa,
                        SqlTrazabilidadConsecutivo,
                        1L,
                        new { Consecutivo = $"%{consecutivoSeguro}%" });

                    consecutivoSeguro = $"{consecutivoSeguro}-{consec.Result:00}";
                }

                DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                {
                    connection.Execute(
                        SpTrdDocumentosIns,
                        new
                        {
                            CodDocumento = NormalizarTexto(CodDocumento),
                            Consecutivo = consecutivoSeguro,
                            IdSobre = (string?)null,
                            IdEstado = 1L,
                            ConfirmaRecepcion = 2,
                            Param1 = (string?)null,
                            Param2 = (string?)null,
                            Fecha = DateTime.Now,
                            Usuario = NormalizarTexto(usuario),
                            CodBarras = NormalizarTexto(CodBarras),
                            Param3 = (string?)null
                        },
                        commandType: System.Data.CommandType.StoredProcedure);

                    return true;
                });
            }
            catch
            {
                // Se conserva comportamiento histórico: los errores de trazabilidad no interrumpen la operación principal.
            }
        }

        private bool fxBeneficiariosNoIncluidos(int CodEmpresa, string plan, int operadora, long contrato)
        {
            try
            {
                var porcentaje = DbHelper.ExecuteSingleQuery(
                    CreatePortalDb(),
                    CodEmpresa,
                    SqlBeneficiariosPorcentaje,
                    0m,
                    new
                    {
                        CodPlan = NormalizarTexto(plan),
                        Operadora = operadora,
                        Contrato = contrato
                    });

                return porcentaje.Code == 0 && porcentaje.Result >= 100;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Valida la cantidad máxima de contratos activos permitidos para la persona y plan.
        /// </summary>
        private void ValidarCantidadContratosActivos(int codEmpresa, ContratosModels contrato, List<string> mensajes)
        {
            var parametros = CrearParametrosContratoBase(contrato);
            if (parametros is null)
            {
                return;
            }
            var maximos = DbHelper.ExecuteSingleQuery(CreatePortalDb(), codEmpresa, SqlPlanContratosActivos, 0, parametros);
            var activos = DbHelper.ExecuteSingleQuery(CreatePortalDb(), codEmpresa, SqlContratosActivosPersona, 0, parametros);

            if (maximos.Code == 0 && activos.Code == 0 && activos.Result >= maximos.Result)
            {
                mensajes.Add(" - Esta persona ha superado el número máximo de contratos activos en este plan... \n");
            }
        }

        /// <summary>
        /// Valida si el estado de la persona aplica para el plan indicado.
        /// </summary>
        private void ValidarEstadoPersonaPlan(int codEmpresa, ContratosModels contrato, List<string> mensajes)
        {
            var parametros = CrearParametrosContratoBase(contrato);
            if (parametros is null)
            {
                return;
            }

            var encontrado = DbHelper.WithConn(CreatePortalDb(), codEmpresa, connection =>
                connection.QueryFirstOrDefault<int>(
                    SqlValidaEstados,
                    parametros));

            if (encontrado.Code == 0 && encontrado.Result == 0)
            {
                mensajes.Add(" - El estado de esta persona no aplica para en este plan o el Plan se encuentra inactivo...\n");
            }
        }

        /// <summary>
        /// Valida plazo, acceso del usuario y existencia de destinos requeridos por plan.
        /// </summary>
        private void ValidarReglasPlan(int codEmpresa, ContratosModels contrato, List<string> mensajes)
        {
            var valida = DbHelper.ExecuteSingleQuery<ValidaContratos>(
                CreatePortalDb(),
                codEmpresa,
                SqlValidacionesContrato,
                new ValidaContratos(),
                CrearParametrosContratoValidacion(contrato));

            if (valida.Result is null)
            {
                return;
            }

            if (valida.Result.plazo_valida == 0)
            {
                mensajes.Add(" - El Plazo se encuentra fuera del Rango Permitido por el Plan...\n");
            }

            if (valida.Result.acceso_valida == 0)
            {
                mensajes.Add(" - El usuario no tiene Autorización para gestionar este Plan...\n");
            }

            if (valida.Result.destinos > 0 && !TieneDestinosIndicados(codEmpresa, contrato))
            {
                mensajes.Add(" - No ha Indicado Ningún Destino/Objetivo para este Plan...\n");
            }
        }

        /// <summary>
        /// Valida campos requeridos del contrato.
        /// </summary>
        private static void ValidarCamposContrato(ContratosModels contrato, List<string> mensajes)
        {
            if (string.IsNullOrWhiteSpace(contrato.cod_plan))
            {
                mensajes.Add(" - Indique el Plan...\n");
            }

            if (string.IsNullOrWhiteSpace(contrato.cedula))
            {
                mensajes.Add(" - Especifique la persona ...\n");
            }

            if (!contrato.porc_deduc.HasValue)
            {
                mensajes.Add(" - El Porcentaje de deducción no es válido...\n");
            }

            if (!contrato.monto.HasValue)
            {
                mensajes.Add(" - La cuota especificada no es válida...\n");
            }

            if (!contrato.inversion.HasValue)
            {
                mensajes.Add(" - La inversión especificada no es válida...\n");
            }

            if (!contrato.plazo.HasValue)
            {
                mensajes.Add(" - El plazo especificado no es válido...\n");
            }

            if (!contrato.inc_anual.HasValue)
            {
                mensajes.Add(" - El % de Incremento anual no es válido...\n");
            }

            if (!contrato.capexc.HasValue)
            {
                mensajes.Add(" - El % de Capitalización no es válido...\n");
            }

            if (contrato.tipo_deduc == "P" && contrato.porc_deduc.HasValue && (contrato.porc_deduc > 100 || contrato.porc_deduc < 0))
            {
                mensajes.Add("El Porcentaje de Deducción no es válido!\n");
            }

            contrato.tasa_referencia ??= 0;

            if (contrato.mTipoDeduc == "P" && (contrato.porcentaje > 100 || contrato.porcentaje < 0))
            {
                mensajes.Add("El Porcentaje de Deducción no es válido!\n");
            }
        }

        /// <summary>
        /// Valida montos y plazos mínimos configurados en el plan.
        /// </summary>
        private void ValidarMontosMinimos(int codEmpresa, ContratosModels contrato, List<string> mensajes)
        {
            if (mensajes.Count > 0)
            {
                return;
            }

            var valida = DbHelper.ExecuteSingleQuery<ValidaContratos>(
                CreatePortalDb(),
                codEmpresa,
                SqlMontosMinimosPlan,
                new ValidaContratos(),
                CrearParametrosContratoBase(contrato));

            if (valida.Result is null)
            {
                return;
            }

            if (contrato.plazo_tipo == "D" && contrato.plazo < valida.Result.plazo_minimo)
            {
                mensajes.Add($" - El Plazo no cumple con el plazo mínimo permitido ({valida.Result.plazo_minimo})...\n");
            }

            if (contrato.plazo_tipo == "M" && contrato.plazo * 30 < valida.Result.plazo_minimo)
            {
                mensajes.Add($" - El Plazo no cumple con el plazo mínimo permitido ({valida.Result.plazo_minimo})...\n");
            }

            var validaMontoMinimo = contrato.tipo_deduc == "M" &&
                valida.Result.cuenta_maestra != 1 &&
                valida.Result.tipo_cdp != 1;
            var validaInversionMinima = contrato.tipo_deduc == "M" &&
                valida.Result.tipo_cdp == 1 &&
                !valida.Result.web_vence.HasValue;

            if (validaMontoMinimo && contrato.monto < valida.Result.monto_minimo)
            {
                mensajes.Add(" - El monto es menor al mínimo permitido...");
            }

            if (validaInversionMinima && contrato.inversion < valida.Result.inversion_minimo)
            {
                mensajes.Add(" - El monto de la INVERSIÓN es menor al mínimo permitido...");
            }
        }

        /// <summary>
        /// Indica si el contrato tiene destinos registrados.
        /// </summary>
        private bool TieneDestinosIndicados(int codEmpresa, ContratosModels contrato)
        {
            // VB6 revisa la tabla en pantalla: un destino elegido aún sin guardar también cuenta
            if (contrato.destinos_pendientes?.Count > 0)
            {
                return true;
            }

            var plan = NormalizarTexto(contrato.cod_plan);
            if (string.IsNullOrWhiteSpace(plan))
            {
                return false;
            }

            var destinos = Fnd_Contratos_Destinos_Obtener(
                codEmpresa,
                contrato.cod_operadora,
                plan,
                contrato.cod_contrato).Result ?? new List<FndContratoDestinoData>();

            return destinos.Any(item => item.id_registro != null && item.id_registro > 0);
        }

        /// <summary>
        /// Valida si el contrato requiere beneficiarios y si estos completan el porcentaje requerido.
        /// </summary>
        private void ValidarBeneficiariosContrato(int codEmpresa, ContratosModels contrato, ErrorDto response)
        {
            var plan = NormalizarTexto(contrato.cod_plan);
            if (string.IsNullOrWhiteSpace(plan))
            {
                return;
            }

            if (fxAplicaBeneficiarios(codEmpresa, plan, contrato.cod_operadora).Result &&
                fxBeneficiariosNoIncluidos(codEmpresa, plan, contrato.cod_operadora, contrato.cod_contrato))
            {
                response.Code = -1;
                response.Description = "No estan incluidos los beneficiarios o el porcentaje es inferior al 100%...Por Favor Incluirlos";
            }
        }

        /// <summary>
        /// Registra la bitácora general del contrato.
        /// </summary>
        private void RegistrarBitacoraContrato(int codEmpresa, string usuario, ContratosModels contrato, string movimiento)
        {
            _Security_MainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = NormalizarTexto(usuario),
                DetalleMovimiento = $"Contrato: {contrato.cod_contrato}  Plan: {NormalizarTexto(contrato.cod_plan)}  Oper: {contrato.cod_operadora}",
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }

        /// <summary>
        /// Crea el consecutivo usado por trazabilidad.
        /// </summary>
        private static string CrearConsecutivoTrazabilidad(ContratosModels contrato) => $"{contrato.cod_contrato}-{NormalizarTexto(contrato.cod_plan)}";

        /// <summary>
        /// Crea parámetros base del contrato para consultas de validación.
        /// </summary>
        private static object? CrearParametrosContratoBase(ContratosModels contrato)
        {
            var codPlan = NormalizarTexto(contrato.cod_plan);
            var cedula = NormalizarTexto(contrato.cedula);

            if (string.IsNullOrWhiteSpace(codPlan) || string.IsNullOrWhiteSpace(cedula))
            {
                return null;
            }

            return new
            {
                Operadora = contrato.cod_operadora,
                CodPlan = codPlan,
                Cedula = cedula
            };
        }

        /// <summary>
        /// Crea parámetros para validaciones adicionales del contrato.
        /// </summary>
        private static object CrearParametrosContratoValidacion(ContratosModels contrato)
        {
            return new
            {
                Operadora = contrato.cod_operadora,
                CodPlan = NormalizarTexto(contrato.cod_plan),
                Plazo = contrato.plazo,
                Usuario = NormalizarTexto(contrato.usuario)
            };
        }

        /// <summary>
        /// Crea los 33 parámetros posicionales de spFnd_Contrato_Registro, en el mismo orden que usa VB6.
        /// </summary>
        private static object CrearParametrosContratoRegistro(ContratosModels contrato, int codigo, string usuario)
        {
            var (cuponFrecuencia, cuponPaga, cuponFrecuenciaId) = ObtenerCuponContrato(contrato);

            return new
            {
                p1 = contrato.cod_operadora,
                p2 = NormalizarTexto(contrato.cod_plan),
                p3 = codigo,
                p4 = NormalizarTexto(contrato.cedula),
                p5 = contrato.cod_vendedor,
                p6 = NormalizarTexto(contrato.tipo_deduc ?? contrato.mTipoDeduc),
                p7 = contrato.porc_deduc,
                p8 = "A",
                p9 = contrato.fecha_corte,
                p10 = contrato.plazo,
                p11 = contrato.monto,
                p12 = contrato.inversion,
                p13 = contrato.renueva,
                p14 = contrato.inc_anual,
                p15 = contrato.inc_tipo,
                p16 = 0,
                p17 = contrato.cod_banco,
                p18 = NormalizarTexto(contrato.cuenta_ahorros),
                p19 = NormalizarTexto(contrato.tipo_pago),
                p20 = contrato.capexc,
                p21 = NormalizarTexto(contrato.albacea_cedula),
                p22 = NormalizarTexto(contrato.albacea_nombre),
                p23 = NormalizarTexto(contrato.plazo_tipo),
                p24 = contrato.tasa_referencia,
                p25 = NormalizarTexto(contrato.tasa_tipo),
                p26 = contrato.tasa_ptsadd,
                p27 = cuponFrecuencia,
                p28 = cuponFrecuenciaId,
                p29 = cuponPaga,
                p30 = contrato.cupon_proximo,
                p31 = contrato.ind_deduccion == true ? 1 : 0,
                p32 = contrato.permite_giro_terceros == true ? 1 : 0,
                p33 = NormalizarTexto(usuario)
            };
        }

        /// <summary>
        /// Define frecuencia, indicador y id de cupón (VB6: pCuponFrecuencia, pCuponPaga, pCuponFrecuenciaId).
        /// </summary>
        private static (string Frecuencia, int Paga, int? FrecuenciaId) ObtenerCuponContrato(ContratosModels contrato)
        {
            if (contrato.inversion > 0 && contrato.pago_cuponescdp == true)
            {
                return (contrato.cupon_frecuencia ?? string.Empty, 1, NormalizarCuponFrecuenciaId(contrato.idcupon_frecuencia));
            }

            return ("N", 0, null);
        }

        /// <summary>
        /// Indica si la tasa debe calcularse mediante función SQL.
        /// </summary>
        private static bool DebeUsarTasaCalculada(bool chkCuponPaga, int tipoCdp) => !chkCuponPaga || tipoCdp == 1;

        private static int? NormalizarCuponFrecuenciaId(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor) ||
                string.Equals(valor.Trim(), "Null", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return int.TryParse(valor.Trim(), out var id) ? id : null;
        }


        #endregion

    }
}
