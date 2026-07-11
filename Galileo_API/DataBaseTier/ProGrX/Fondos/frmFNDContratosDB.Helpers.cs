using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public partial class FrmFndContratosDB
    {

        private const string SpValidaEstados = "spFND_ValidaEstados";
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
                           INVERSION_MINIMO
                    FROM dbo.fnd_Planes
                    WHERE cod_operadora = @Operadora
                      AND cod_plan = @CodPlan;";

        private const string SqlInsertContrato = @"
                    INSERT INTO dbo.FND_Contratos
                    (
                        Cod_operadora,
                        Cod_plan,
                        Cod_Contrato,
                        Cedula,
                        Cod_Vendedor,
                        Tipo_Deduc,
                        PORC_DEDUC,
                        Estado,
                        Fecha_Inicio,
                        Plazo,
                        Monto,
                        Renueva,
                        Inc_Anual,
                        Inc_Tipo,
                        Ind_comision,
                        Cod_Banco,
                        Cuenta_Ahorros,
                        Tipo_Pago,
                        CapExc,
                        rend_corte,
                        rend_saldo,
                        fecha_corte,
                        usuario,
                        albacea_cedula,
                        albacea_nombre,
                        plazo_tipo,
                        inversion,
                        tasa_referencia,
                        Tasa_Tipo,
                        Tasa_PtsAdd,
                        Cupon_Frecuencia,
                        Cupon_Proximo,
                        Cupon_Consec,
                        ind_deduccion,
                        PERMITE_GIRO_TERCEROS,
                        IDCUPON_FRECUENCIA,
                        PAGO_CUPONESCDP,
                        ID_PER_TASA
                    )
                    VALUES
                    (
                        @cod_operadora,
                        @cod_plan,
                        @cod_contrato,
                        @cedula,
                        @cod_vendedor,
                        @tipo_deduc,
                        @porc_deduc,
                        @estado,
                        @fecha_inicio,
                        @plazo,
                        @monto,
                        @renueva,
                        @inc_anual,
                        @inc_tipo,
                        @ind_comision,
                        @cod_banco,
                        @cuenta_ahorros,
                        @tipo_pago,
                        @cap_exc,
                        @rend_corte,
                        @rend_saldo,
                        @fecha_corte,
                        @usuario,
                        @albacea_cedula,
                        @albacea_nombre,
                        @plazo_tipo,
                        @inversion,
                        @tasa_referencia,
                        @tasa_tipo,
                        @tasa_ptsadd,
                        @cupon_frecuencia,
                        @cupon_proximo,
                        @cupon_consec,
                        @ind_deduccion,
                        @permite_giro_terceros,
                        @idcupon_frecuencia,
                        @pago_cuponescdp,
                        dbo.fxFnd_ReglaId_Tasa(@cod_plan, @fecha_inicio)
                    );";

        private const string SqlUpdateContrato = @"
                    UPDATE dbo.FND_Contratos
                    SET cod_Vendedor = @cod_vendedor,
                        Plazo = @plazo,
                        fecha_corte = @fecha_corte,
                        Monto = @monto,
                        Renueva = @renueva,
                        Inc_Anual = @inc_anual,
                        Inc_Tipo = @inc_tipo,
                        Cod_Banco = @cod_banco,
                        Cuenta_Ahorros = @cuenta_ahorros,
                        tipo_Pago = @tipo_pago,
                        CapExc = @cap_exc,
                        albacea_Cedula = @albacea_cedula,
                        albacea_nombre = @albacea_nombre,
                        plazo_tipo = @plazo_tipo,
                        inversion = @inversion,
                        tasa_referencia = @tasa_referencia,
                        modifica_fecha = dbo.MyGetdate(),
                        modifica_usuario = @modifica_usuario,
                        cupon_frecuencia = @cupon_frecuencia,
                        cupon_proximo = @cupon_proximo,
                        ind_deduccion = @ind_deduccion,
                        PERMITE_GIRO_TERCEROS = @permite_giro_terceros,
                        Tipo_Deduc = @tipo_deduc,
                        PORC_DEDUC = @porc_deduc,
                        IDCUPON_FRECUENCIA = @idcupon_frecuencia,
                        PAGO_CUPONESCDP = @pago_cuponescdp,
                        ID_PER_TASA = dbo.fxFnd_ReglaId_Tasa(@cod_plan, @fecha_inicio)
                    WHERE cod_operadora = @cod_operadora
                      AND cod_plan = @cod_plan
                      AND cod_Contrato = @cod_contrato;";

        private const string SqlConsecutivoContrato = @"
                    SELECT ISNULL(Consecutivo, 0) + 1
                    FROM dbo.fnd_planes
                    WHERE cod_operadora = @Operadora
                      AND cod_plan = @CodPlan;";

        private const string SqlUpdateConsecutivoContrato = @"
                    UPDATE dbo.fnd_planes
                    SET Consecutivo = @NuevoConsecutivo
                    WHERE cod_operadora = @Operadora
                      AND cod_plan = @CodPlan;";

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
        /// Inserta un contrato nuevo y registra bitácora, cambios y trazabilidad.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="usuario">Usuario que registra el contrato.</param>
        /// <param name="contrato">Datos del contrato.</param>
        /// <returns>Resultado de la inserción.</returns>
        private ErrorDto insertarContrato(int CodEmpresa, string usuario, ContratosModels contrato)
        {
            var response = DbHelper.CreateOkResponse();

            try
            {
                var plan = NormalizarTexto(contrato.cod_plan);
                if (string.IsNullOrWhiteSpace(plan))
                {
                    return DbHelper.ErrorResponse("El código del plan es requerido.", -2);
                }

                contrato.cod_contrato = fxConsecutivoContrato(CodEmpresa, contrato.cod_operadora, plan).Result;

                var result = DbHelper.ExecuteNonQuery(
                    CreatePortalDb(),
                    CodEmpresa,
                    SqlInsertContrato,
                    CrearParametrosContratoInsert(contrato));

                if (result.Code != 0)
                {
                    return result;
                }

                ValidarBeneficiariosContrato(CodEmpresa, contrato, response);
                if (response.Code != 0)
                {
                    return response;
                }

                RegistrarBitacoraContrato(CodEmpresa, usuario, contrato, "Registra - WEB");
                RegistrarTagContrato(contrato, usuario);
                sbGuardaCambios(CodEmpresa, contrato.cod_operadora, plan, contrato.cod_contrato, usuario, 05, $"Mensualidad: {contrato.monto} ¦ Inversión: {contrato.inversion}");
                sbTrazabilidad_Inserta(CodEmpresa, "04", CrearConsecutivoTrazabilidad(contrato), contrato.cod_contrato.ToString(), usuario);
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Actualiza un contrato existente y registra bitácora, cambios y trazabilidad.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="usuario">Usuario que actualiza el contrato.</param>
        /// <param name="vCambios">Valores anteriores usados para registrar cambios.</param>
        /// <param name="contrato">Datos actualizados del contrato.</param>
        /// <returns>Resultado de la actualización.</returns>
        private ErrorDto actualizarContrato(int CodEmpresa, string usuario, FndCambios vCambios, ContratosModels contrato)
        {
            var response = DbHelper.CreateOkResponse();

            try
            {
                var result = DbHelper.ExecuteNonQuery(
                    CreatePortalDb(),
                    CodEmpresa,
                    SqlUpdateContrato,
                    CrearParametrosContratoUpdate(contrato));

                if (result.Code != 0)
                {
                    return result;
                }

                RegistrarBitacoraContrato(CodEmpresa, usuario, contrato, "Modifica - WEB");
                RegistrarCambiosContrato(CodEmpresa, usuario, vCambios, contrato);
                sbTrazabilidad_Inserta(CodEmpresa, "04", CrearConsecutivoTrazabilidad(contrato), contrato.cod_contrato.ToString(), usuario);
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Obtiene y actualiza el consecutivo siguiente para contratos del plan.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="operadora">Código de operadora.</param>
        /// <param name="plan">Código del plan.</param>
        /// <returns>Consecutivo disponible para el contrato.</returns>
        private ErrorDto<int> fxConsecutivoContrato(int CodEmpresa, int operadora, string plan)
        {
            var response = DbHelper.CreateOkResponse(0);

            try
            {
                var parametros = new
                {
                    Operadora = operadora,
                    CodPlan = NormalizarTexto(plan)
                };

                var consecutivo = DbHelper.ExecuteSingleQuery(
                    CreatePortalDb(),
                    CodEmpresa,
                    SqlConsecutivoContrato,
                    0,
                    parametros);

                if (consecutivo.Code != 0)
                {
                    return consecutivo;
                }

                if (consecutivo.Result <= 0)
                {
                    return response;
                }

                var update = DbHelper.ExecuteNonQuery(
                    CreatePortalDb(),
                    CodEmpresa,
                    SqlUpdateConsecutivoContrato,
                    new
                    {
                        NuevoConsecutivo = consecutivo.Result,
                        Operadora = operadora,
                        CodPlan = NormalizarTexto(plan)
                    });

                if (update.Code != 0)
                {
                    response.Code = update.Code;
                    response.Description = update.Description;
                    return response;
                }

                response.Result = consecutivo.Result;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = 0;
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
                            PlazoInversion = NormalizarTexto(param.PlazoInversion),
                            CuponFrecuencia = NormalizarTexto(param.CuponFrecuencia)
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
                    SpValidaEstados,
                    parametros,
                    commandType: System.Data.CommandType.StoredProcedure));

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

            if (contrato.tipo_deduc == "M" && contrato.monto < valida.Result.monto_minimo)
            {
                mensajes.Add(" - El monto es menor al mínimo permitido...");
            }

            if (contrato.tipo_deduc == "M" && contrato.inversion < valida.Result.inversion_minimo)
            {
                mensajes.Add(" - El monto de la INVERSIÓN es menor al mínimo permitido...");
            }
        }

        /// <summary>
        /// Indica si el contrato tiene destinos registrados.
        /// </summary>
        private bool TieneDestinosIndicados(int codEmpresa, ContratosModels contrato)
        {
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
        /// Registra el tag de seguimiento del contrato.
        /// </summary>
        private void RegistrarTagContrato(ContratosModels contrato, string usuario)
        {
            _mProGrxMain.SbSIFRegistraTags(new SifRegistraTagsRequestDto
            {
                Codigo = NormalizarTexto(contrato.cod_plan),
                Tag = "S09",
                Observacion = "Fondos",
                Documento = contrato.cod_contrato.ToString(),
                Modulo = "FND",
                Llave_01 = NormalizarTexto(contrato.cod_plan),
                Llave_02 = contrato.cod_contrato.ToString(),
                Llave_03 = NormalizarTexto(contrato.cedula),
                Usuario = NormalizarTexto(usuario)
            });
        }

        /// <summary>
        /// Registra cambios comparando valores anteriores contra valores actuales del contrato.
        /// </summary>
        private void RegistrarCambiosContrato(int codEmpresa, string usuario, FndCambios cambios, ContratosModels contrato)
        {
            var plan = NormalizarTexto(contrato.cod_plan);
            var usuarioSeguro = NormalizarTexto(usuario);
            if (string.IsNullOrWhiteSpace(plan) || string.IsNullOrWhiteSpace(usuarioSeguro))
            {
                return;
            }
            if (cambios.vCuota != contrato.monto)
            {
                sbGuardaCambios(codEmpresa, contrato.cod_operadora, plan, contrato.cod_contrato, usuario, 01, $"Anterior {cambios.vCuota} - Nueva {contrato.monto} ");
            }

            if (cambios.vPlazo != contrato.plazo)
            {
                sbGuardaCambios(codEmpresa, contrato.cod_operadora, plan, contrato.cod_contrato, usuario, 02, $"Anterior {cambios.vPlazo} - Nueva {contrato.plazo} ");
            }

            if (cambios.vInversion != contrato.inversion)
            {
                sbGuardaCambios(codEmpresa, contrato.cod_operadora, plan, contrato.cod_contrato, usuario, 03, $"Anterior {cambios.vInversion} - Nueva {contrato.inversion} ");
            }

            if (cambios.vDedPlanilla != contrato.ind_deduccion)
            {
                var anterior = cambios.vDedPlanilla ? "SI" : "NO";
                var nuevo = contrato.ind_deduccion.HasValue && contrato.ind_deduccion.Value ? "SI" : "NO";
                sbGuardaCambios(codEmpresa, contrato.cod_operadora, plan, contrato.cod_contrato, usuario, 06, $"Anterior {anterior} - Nueva {nuevo} ");
            }
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
        /// Crea los parámetros de inserción del contrato.
        /// </summary>
        private object CrearParametrosContratoInsert(ContratosModels contrato)
        {
            return CrearParametrosContrato(contrato, "A", NormalizarTexto(contrato.usuario));
        }

        /// <summary>
        /// Crea los parámetros de actualización del contrato.
        /// </summary>
        private object CrearParametrosContratoUpdate(ContratosModels contrato)
        {
            return CrearParametrosContrato(
                contrato,
                NormalizarTexto(contrato.estado),
                NormalizarTexto(contrato.usuario));
        }

        /// <summary>
        /// Crea parámetros comunes para insertar o actualizar contratos.
        /// </summary>
        private object CrearParametrosContrato(ContratosModels contrato, string? estado, string? usuario)
        {
            var usuarioSeguro = NormalizarTexto(usuario);
            var estadoSeguro = NormalizarTexto(estado);
            return new
            {
                cod_operadora = contrato.cod_operadora,
                cod_plan = NormalizarTexto(contrato.cod_plan),
                cod_contrato = contrato.cod_contrato,
                cedula = NormalizarTexto(contrato.cedula),
                cod_vendedor = contrato.cod_vendedor,
                tipo_deduc = NormalizarTexto(contrato.tipo_deduc),
                porc_deduc = contrato.porc_deduc,
                estado = estadoSeguro,
                fecha_inicio = contrato.fecha_inicio,
                plazo = contrato.plazo,
                monto = contrato.monto,
                renueva = contrato.renueva,
                inc_anual = contrato.inc_anual,
                inc_tipo = contrato.inc_tipo,
                ind_comision = 0,
                cod_banco = contrato.cod_banco,
                cuenta_ahorros = NormalizarTexto(contrato.cuenta_ahorros),
                tipo_pago = NormalizarTexto(contrato.tipo_pago),
                cap_exc = contrato.capexc,
                rend_corte = 0,
                rend_saldo = 0,
                fecha_corte = contrato.fecha_corte,
                usuario = usuarioSeguro,
                albacea_cedula = NormalizarTexto(contrato.albacea_cedula),
                albacea_nombre = NormalizarTexto(contrato.albacea_nombre),
                plazo_tipo = NormalizarTexto(contrato.plazo_tipo),
                inversion = contrato.inversion,
                tasa_referencia = contrato.tasa_referencia,
                tasa_tipo = NormalizarTexto(contrato.tasa_tipo),
                tasa_ptsadd = contrato.tasa_ptsadd,
                cupon_frecuencia = pCuponFrecuencia,
                cupon_proximo = contrato.cupon_proximo,
                cupon_consec = 0,
                ind_deduccion = contrato.ind_deduccion,
                permite_giro_terceros = contrato.permite_giro_terceros,
                idcupon_frecuencia = pCuponFrecuenciaId,
                pago_cuponescdp = pCuponPaga,
                modifica_usuario = usuarioSeguro
            };
        }

        /// <summary>
        /// Indica si la tasa debe calcularse mediante función SQL.
        /// </summary>
        private static bool DebeUsarTasaCalculada(bool chkCuponPaga, int tipoCdp) => !chkCuponPaga || tipoCdp == 1;


        #endregion

    }
}