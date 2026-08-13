using Dapper;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_EstudioCrd;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_EstudioCrd
{
    public partial class FrmPreaEstudiov2DB
    {
        /// <summary>
        /// Guarda el preanálisis (nuevo expediente, nuevo sub-expediente, o modificación).
        /// VB6: fxGuardar (frmPreaEstudiov2.frm, línea ~12313):
        ///   1. fxValidaDatos (validaciones en memoria, replicadas en BL/aquí donde aplica).
        ///   2. EXEC spCrdFormaliza_Valida_Rangos (no bloqueante; su Mensaje se guarda en
        ///      CUMPLIMIENTO_NOTAS, sobreescribiendo lo que el usuario haya escrito).
        ///   3. Según modo: sbEstudio_Guarda_Nuevo (spCrdPreaPreanalisisNuevo) o
        ///      sbEstudio_Guarda_Modifica (spCrdPreaPreanalisisModifica).
        ///   4. Si es nuevo: inicializa Refundiciones/Fianzas/CreditosTransito/ImportCreditosVigentes.
        ///   5. Fix final: UPDATE CUMPLIMIENTO_NOTAS/MONTO_POLIZA_DESEMPLEO/APL_POLIZA_DESEMPLEO.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2GuardarResponse> Prea_frmPreaEstudiov2_Guardar(
            int codEmpresa,
            FrmPreaEstudiov2GuardarRequest request)
        {
            var response = new ErrorDto<FrmPreaEstudiov2GuardarResponse>
            {
                Code = 0,
                Description = "Ok",
                Result = new FrmPreaEstudiov2GuardarResponse()
            };

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);
                connection.Open();

                var usuario = request.usuario?.Trim() ?? string.Empty;
                var codPreanalisisActual = request.cod_preanalisis?.Trim() ?? string.Empty;
                var esNuevo = string.IsNullOrEmpty(codPreanalisisActual);

                // VB6: spCrdFormaliza_Valida_Rangos '<cedula>','<linea>',<monto>,<tasa>,<plazo>,'<destino>','<garantia>',0
                // No bloquea el guardado; su mensaje reemplaza CUMPLIMIENTO_NOTAS.
                var notasCumplimiento = ValidarRangosFormalizacion(connection, request);

                string codPreanalisisResultado;

                if (esNuevo)
                {
                    codPreanalisisResultado = GuardarNuevo(connection, usuario, request);

                    if (!string.IsNullOrEmpty(codPreanalisisResultado))
                    {
                        InicializarTablasHijasNuevoExpediente(connection, codPreanalisisResultado);
                    }
                }
                else
                {
                    // VB6: sbEstudio_Guarda_Modifica (spCrdPreaPreanalisisModifica, ~60 parámetros
                    // posicionales). La mayoría de esos parámetros pertenecen a datos de otras
                    // pestañas (Deducciones, Refundiciones, Desembolsos, Fianzas, liquidez, CIC,
                    // etc.) que Angular aún no consolida en un solo request y que se auditarán
                    // pestaña por pestaña. Por ahora se mantiene la llamada existente (parámetros
                    // nombrados, solo los campos que si están confirmados) — pendiente de
                    // completar cuando se audite cada pestaña restante.
                    codPreanalisisResultado = GuardarModifica(connection, usuario, request, notasCumplimiento);
                }

                // VB6: FIX TEMPORAL DE COLUMNAS NUEVAS (fxGuardar, al final, ambos modos).
                if (!string.IsNullOrEmpty(codPreanalisisResultado))
                {
                    AplicarFixColumnasFinal(connection, codPreanalisisResultado, notasCumplimiento, request);
                }

                response.Result = new FrmPreaEstudiov2GuardarResponse
                {
                    cod_preanalisis = codPreanalisisResultado,
                    estado = "G",
                    estado_desc = esNuevo ? "Registrado" : "Actualizado",
                    mensaje = esNuevo
                        ? "La información fue registrada correctamente."
                        : "La información fue actualizada correctamente."
                };

                return response;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new FrmPreaEstudiov2GuardarResponse();
                return response;
            }
        }

        /// <summary>
        /// EXEC spCrdFormaliza_Valida_Rangos '&lt;cedula&gt;','&lt;linea&gt;',&lt;monto&gt;,
        /// &lt;tasa&gt;,&lt;plazo&gt;,'&lt;destino&gt;','&lt;garantia&gt;',0
        /// (fxGuardar, línea ~12333). No bloquea el guardado — el campo Mensaje resultante
        /// reemplaza lo que hubiera en CUMPLIMIENTO_NOTAS.
        /// </summary>
        private static string ValidarRangosFormalizacion(IDbConnection connection, FrmPreaEstudiov2GuardarRequest request)
        {
            try
            {
                const string sql = @"EXEC spCrdFormaliza_Valida_Rangos
                    @Cedula, @Linea, @Monto, @Tasa, @Plazo, @Destino, @Garantia, 0";
                var row = connection.QueryFirstOrDefault(sql, new
                {
                    Cedula = request.cedula?.Trim() ?? string.Empty,
                    Linea = request.linea?.Trim() ?? string.Empty,
                    request.monto,
                    request.tasa,
                    request.plazo,
                    Destino = request.destino?.Trim() ?? string.Empty,
                    Garantia = request.garantia?.Trim() ?? string.Empty
                }) as IDictionary<string, object>;
                if (row is null)
                {
                    return string.Empty;
                }

                var dict = new Dictionary<string, object>(row, StringComparer.OrdinalIgnoreCase);
                return GetString(dict, "Mensaje");
            }
            catch
            {
                // No bloqueante en VB6 tampoco: si falla, simplemente no hay mensaje de validación.
                return string.Empty;
            }
        }

        /// <summary>
        /// EXEC spCrdPreaPreanalisisNuevo con los 43 parámetros posicionales confirmados en
        /// sbEstudio_Guarda_Nuevo (frmPreaEstudiov2.frm, línea ~12156). Simplificaciones
        /// documentadas (no inventadas, son el comportamiento por defecto real de VB6 cuando
        /// el dato no está disponible en el formulario):
        ///   - TASA_PTS_BONO: VB6 lo inicializa en 0 (línea 10658) y solo lo recalcula tras
        ///     validar membresía de socio (lógica de bono no trazada aún) — se usa 0.
        ///   - pOficina: EXEC sbSIFOficinasUsuario '&lt;usuario&gt;' (campo Titular), igual que
        ///     VB6 al iniciar sesión (mProGrX_Main.bas).
        /// </summary>
        private static string GuardarNuevo(
            IDbConnection connection,
            string usuario,
            FrmPreaEstudiov2GuardarRequest request)
        {
            var esSubExpediente = string.Equals(request.tipo_preanalisis, "S", StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(request.cod_preanalisis_ref);

            var tipoExpediente = esSubExpediente ? "S" : "E";
            var expedienteRef = esSubExpediente ? request.cod_preanalisis_ref.Trim() : null;

            var oficina = ObtenerOficinaTitular(connection, usuario);
            var edad = CalcularEdad(request.fecha_nacimiento);
            var cph = string.IsNullOrWhiteSpace(request.cph) ? "0" : request.cph.Trim();
            const int tasaPtsBono = 0; // ver nota en el comentario del método

            var fechaNacimiento = request.fecha_nacimiento.HasValue
                ? request.fecha_nacimiento.Value.ToString("yyyy-MM-dd")
                : string.Empty;

            const string sql = @"EXEC spCrdPreaPreanalisisNuevo
                @TipoSalario, @TipoExpediente, @ExpedienteRef, @Usuario, @Cedula,
                @Linea, @Destino, @Nombre, @Sexo, @FechaNacimiento, @PolizaVida,
                @PolizaIncendio, @PrimeraCuota, @Monto, @Tasa, @Plazo, @Cuota,
                @MontoPolizaVida, @MontoPolizaIncendio, @Compromiso, NULL,
                @Garantia, @Garantia, @Fiadores, NULL, @Oficina, @TasaPtsBono,
                NULL, @Edad, @EdadAplica, @EdadJustificacion, @Plazo, 0, 0, 0,
                @Cph, 1, @MontoConstruccion, @PolizaVehiculo, @MontoPolizaPrenda,
                @ValorPrenda, @ClasificacionCrediticia, @NoOpCrm";
            var row = connection.QueryFirstOrDefault(sql, new
            {
                TipoSalario = request.tipo_salario?.Trim() ?? string.Empty,
                TipoExpediente = tipoExpediente,
                ExpedienteRef = expedienteRef,
                Usuario = usuario,
                Cedula = request.cedula?.Trim() ?? string.Empty,
                Linea = request.linea?.Trim() ?? string.Empty,
                Destino = request.destino?.Trim() ?? string.Empty,
                Nombre = request.nombre?.Trim() ?? string.Empty,
                Sexo = string.IsNullOrEmpty(request.sexo) ? string.Empty : request.sexo.Trim().Substring(0, 1),
                FechaNacimiento = fechaNacimiento,
                PolizaVida = BoolToInt(request.poliza_vida),
                PolizaIncendio = BoolToInt(request.poliza_incendio),
                PrimeraCuota = BoolToInt(request.primera_cuota),
                request.monto,
                request.tasa,
                request.plazo,
                request.cuota,
                MontoPolizaVida = request.monto_poliza_vida,
                MontoPolizaIncendio = request.monto_poliza_incendio,
                request.compromiso,
                Garantia = request.garantia?.Trim() ?? string.Empty,
                request.fiadores,
                Oficina = oficina,
                TasaPtsBono = tasaPtsBono,
                Edad = edad,
                EdadAplica = request.edad_aplica,
                EdadJustificacion = request.edad_justificacion ?? string.Empty,
                Cph = cph,
                MontoConstruccion = request.monto_construccion,
                PolizaVehiculo = BoolToInt(request.poliza_vehiculo),
                MontoPolizaPrenda = request.monto_poliza_prenda,
                ValorPrenda = request.valor_prenda,
                ClasificacionCrediticia = request.clasificacion_crediticia ?? string.Empty,
                NoOpCrm = request.no_op_crm?.Trim() ?? string.Empty
            }) as IDictionary<string, object>;
            if (row is null)
            {
                return string.Empty;
            }

            var dict = new Dictionary<string, object>(row, StringComparer.OrdinalIgnoreCase);
            return GetString(dict, "cod_preanalisis");
        }

        /// <summary>
        /// EXEC spCrdPreaPreanalisisModifica — VERSIÓN PARCIAL, PERO CON NOMBRES DE PARÁMETRO
        /// CORREGIDOS. El SP real (firma confirmada por el usuario, no adivinada) declara ~85
        /// parámetros con estos nombres exactos (p.ej. @COD_PREANALISIS, @USUARIO, @EXTRAS_FIJAS...);
        /// la versión anterior de este método usaba alias inventados (@Expediente, @Usuario...) que
        /// NO coinciden con ningún parámetro real del SP, así que cada llamada a "Modificar" (editar
        /// un expediente existente) fallaba en tiempo de ejecución con
        /// "Procedure expects parameter '@COD_PREANALISIS', which was not supplied" (capturado
        /// silenciosamente por el catch de Prea_frmPreaEstudiov2_Guardar). Este fix corrige los
        /// nombres para los ~15 campos que Angular sí consolida hoy.
        ///
        /// Pendiente real: el SP tiene ~85 parámetros sin valor por defecto (los únicos con
        /// "= NULL"/"= 0" son PORCENTAJE_LIBRE, GARANTIA, COD_GARANTIA, COD_ENDEUDAMIENTO,
        /// COD_HISTORIAL, COD_MORA, COD_CAPACIDAD, SALARIO_REAL, DEVENGADO_MES, NSUB_EXP,
        /// GARANTIA_FND, COD_OFICINA, TASA_PTS_BONO, GARANTIA_FND_CONTRATO, EDAD_PERSONA,
        /// PTS_EXTRA_FAP, ID_COMPONENTE_AD, COD_FORMULARIO_CPH, CODPRM_COMPAD,
        /// MONTO_CONSTRUCCION, NUM_OPORT_CRM, APL_POLIZA_DESEMPLEO, MONTO_POLIZA_DESEMPLEO).
        /// Decenas de parámetros obligatorios (TOTAL_CARGA_CCSS, CARGA_CCSS, CARGA_ASOCIACION,
        /// CARGA_FRAP, CARGA_IMPUESTO_SALARIO, DEDUCCIONES, CRD_TRANSITO_CANCELADOS/XCOBRAR,
        /// SALARIO_LIQUIDO, REFUNDICIONES(_CUOTA), DESEMBOLSOS(_CUOTA), LIQUIDO_TOTAL,
        /// LIQUIDEZ_SIMPLE/CFIANZAS(_CA), FIANZAS, CONDICION_EMBARGO, PORC_LIQ_*, UN_PAGO_X_MES,
        /// TASA_ESTRESADA(_PTS), MONTO_PORC_COMPONENTE_AD, APL_IND_COMPONENTE, PUNTOS_CIC_DEUDOR,
        /// NIVEL_COMPORTAMIENTO_HIST, DIAS_INTERES_GASTOS_OP, INDICADOR_EDITABLE,
        /// IND_TIPO_SALARIO_EXT, COD_ESTADO_V2, MONTO_LIQ_FIADOR_EXT, APLICA_POLIZA_VEHICULO,
        /// MONTO_POLIZA_VEHICULO, MONTO_VALOR_VEHICULO, COD_CATEGORIA_ASOCIADO, ID_PROMOTOR,
        /// MONTO_MEJORA_CUOTA, MONTO_INTERES, MONTO_COMISION, MEJORA_CUOTA, REFUNDICIONES_MORA,
        /// SALARIO_USURA, SALARIO_NORMATIVA) todavía no se envían porque provienen de pestañas
        /// (Deducciones, Refundiciones, Desembolsos, Fianzas, liquidez, CIC) que Angular audita
        /// por separado y aún no consolida en un solo payload de guardado. Hasta completarlo, el
        /// SP seguirá fallando salvo que la base de datos tenga defaults reales fuera de lo que
        /// muestra esta firma (no verificable desde aquí). No se adivinan esos ~70 valores.
        ///
        /// También se detectó que @FIADORES y @CONTRATO (los alias @Fiadores/@Contrato previos)
        /// NO existen como tales en el SP real — se omiten aquí en vez de adivinar a qué parámetro
        /// corresponden (candidatos posibles no confirmados: GARANTIA_FND_CONTRATO para contrato).
        /// </summary>
        private static string GuardarModifica(
            IDbConnection connection,
            string usuario,
            FrmPreaEstudiov2GuardarRequest request,
            string notasCumplimiento)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@COD_PREANALISIS", request.cod_preanalisis?.Trim() ?? string.Empty, DbType.String);
            parameters.Add("@TIPO_PREANALISIS", string.IsNullOrEmpty(request.tipo_preanalisis) ? "E" : request.tipo_preanalisis.Trim(), DbType.String);
            parameters.Add("@COD_PREANALISIS_REF", string.IsNullOrWhiteSpace(request.cod_preanalisis_ref) ? null : request.cod_preanalisis_ref.Trim(), DbType.String);
            parameters.Add("@USUARIO", usuario, DbType.String);
            parameters.Add("@CEDULA", request.cedula?.Trim() ?? string.Empty, DbType.String);
            parameters.Add("@NOMBRE", request.nombre?.Trim() ?? string.Empty, DbType.String);
            parameters.Add("@SEXO", request.sexo?.Trim() ?? string.Empty, DbType.String);
            parameters.Add("@FECHA_NACIMIENTO", request.fecha_nacimiento, DbType.Date);
            parameters.Add("@COD_LINEA", request.linea?.Trim() ?? string.Empty, DbType.String);
            parameters.Add("@COD_DESTINO", request.destino?.Trim() ?? string.Empty, DbType.String);
            parameters.Add("@GARANTIA", request.garantia?.Trim() ?? string.Empty, DbType.String);
            parameters.Add("@COD_GARANTIA", request.garantia?.Trim() ?? string.Empty, DbType.String);
            parameters.Add("@APL_POLIZA_VIDA", BoolToInt(request.poliza_vida), DbType.Int32);
            parameters.Add("@APL_POLIZA_INCENDIO", BoolToInt(request.poliza_incendio), DbType.Int32);
            parameters.Add("@APL_PRIMER_CUOTA", BoolToInt(request.primera_cuota), DbType.Int32);
            parameters.Add("@FECHA_CORTE_COLIILA", request.corte_colilla, DbType.Date);
            parameters.Add("@MONTO", request.monto, DbType.Decimal);
            parameters.Add("@TASA", request.tasa, DbType.Decimal);
            parameters.Add("@PLAZO", request.plazo, DbType.Int32);
            parameters.Add("@CUOTA", request.cuota, DbType.Decimal);
            parameters.Add("@MONTO_POLIZA_VIDA", request.monto_poliza_vida, DbType.Decimal);
            parameters.Add("@MONTO_POLIZA_INCENDIO", request.monto_poliza_incendio, DbType.Decimal);
            parameters.Add("@COMPROMISO", request.compromiso, DbType.Decimal);
            parameters.Add("@SALARIO_DEVENGADO_COLILLA", request.salario_devengado, DbType.Decimal);
            parameters.Add("@EXTRAS_FIJAS", request.componente_adicional_base, DbType.Decimal);
            parameters.Add("@SALARIO_CONSTANCIA", request.salario_constancia, DbType.Decimal);
            parameters.Add("@SALARIO_ORDEN_PATRONAL", request.salario_orden_patronal, DbType.Decimal);
            parameters.Add("@MONTO_ACT_PRIVADAS", request.ingreso_privado, DbType.Decimal);
            parameters.Add("@ID_COMPONENTE_AD", request.componente_adicional_id, DbType.Int32);
            parameters.Add("@PORCENTAJE_COMPONENTE_AD", request.componente_adicional_porc, DbType.Decimal);
            parameters.Add("@DEVENGADO_MES", request.salario_mensual, DbType.Decimal);
            parameters.Add("@MONTO_CONSTRUCCION", request.monto_construccion, DbType.Decimal);
            parameters.Add("@TIPO_SALARIO", request.tipo_salario?.Trim() ?? string.Empty, DbType.String);
            parameters.Add("@NUM_OPORT_CRM", string.IsNullOrWhiteSpace(request.no_op_crm) ? null : request.no_op_crm.Trim(), DbType.String);
            parameters.Add("@CUMPLIMIENTO_NOTAS", notasCumplimiento, DbType.String);

            var codPreanalisis = connection.QueryFirstOrDefault<string>(
                "spCrdPreaPreanalisisModifica",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return codPreanalisis ?? request.cod_preanalisis?.Trim() ?? string.Empty;
        }

        /// <summary>
        /// VB6 (fxGuardar, solo para NuevoRegistro, línea ~12376): inicializa las tablas
        /// hijas del expediente recién creado.
        /// </summary>
        private static void InicializarTablasHijasNuevoExpediente(IDbConnection connection, string codPreanalisis)
        {
            var parameters = new { Expediente = codPreanalisis };

            try { connection.Execute("EXEC spCRDPreaRefundiciones @Expediente, 'I'", parameters); } catch { /* no bloqueante, igual que VB6 (solo MsgBox) */ }
            try { connection.Execute("EXEC spCRDPreaFianzas @Expediente, 'I'", parameters); } catch { /* no bloqueante */ }
            try { connection.Execute("EXEC spCRDPreaCreditosTransito @Expediente, 'I', 0", parameters); } catch { /* no bloqueante */ }

            // VB6: m_NumPagos por defecto es 2 (línea 15393); solo cambia tras cargar datos
            // de frecuencia de pago del socio, que Angular no rastrea todavía.
            try { connection.Execute("EXEC spCRDPreaImportCreditosVigentes @Expediente, 2", parameters); } catch { /* no bloqueante */ }
        }

        /// <summary>
        /// VB6 (fxGuardar, "FIX TEMPORAL DE COLUMNAS NUEVAS", línea ~12421-12432): se ejecuta
        /// siempre, tanto para nuevo como para modificar.
        /// </summary>
        private static void AplicarFixColumnasFinal(
            IDbConnection connection,
            string codPreanalisis,
            string notasCumplimiento,
            FrmPreaEstudiov2GuardarRequest request)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@Notas", notasCumplimiento ?? string.Empty, DbType.String);
                parameters.Add("@MontoPolizaDesempleo", request.monto_poliza_desempleo, DbType.Decimal);
                parameters.Add("@AplPolizaDesempleo", BoolToInt(request.poliza_desempleo), DbType.Int32);
                parameters.Add("@Expediente", codPreanalisis, DbType.String);

                connection.Execute(
                    @"UPDATE CRD_PREA_PREANALISIS
                      SET CUMPLIMIENTO_NOTAS = @Notas,
                          MONTO_POLIZA_DESEMPLEO = @MontoPolizaDesempleo,
                          APL_POLIZA_DESEMPLEO = @AplPolizaDesempleo
                      WHERE cod_Preanalisis = @Expediente",
                    parameters);
            }
            catch
            {
                // No bloqueante, igual que VB6.
            }
        }

        /// <summary>EXEC sbSIFOficinasUsuario '&lt;usuario&gt;' -> campo Titular. VB6:
        /// GLOBALES.gOficinaTitular, asignado al iniciar sesión (mProGrX_Main.bas línea 2887).</summary>
        private static string ObtenerOficinaTitular(IDbConnection connection, string usuario)
        {
            if (string.IsNullOrWhiteSpace(usuario))
            {
                return string.Empty;
            }

            try
            {
                const string sql = "EXEC sbSIFOficinasUsuario @Usuario";
                var row = connection.QueryFirstOrDefault(sql, new { Usuario = usuario }) as IDictionary<string, object>;
                if (row is null)
                {
                    return string.Empty;
                }

                var dict = new Dictionary<string, object>(row, StringComparer.OrdinalIgnoreCase);
                return GetString(dict, "Titular");
            }
            catch
            {
                return string.Empty;
            }
        }

        private static int CalcularEdad(DateTime? fechaNacimiento)
        {
            if (!fechaNacimiento.HasValue)
            {
                return 0;
            }

            var hoy = DateTime.Today;
            var edad = hoy.Year - fechaNacimiento.Value.Year;
            if (fechaNacimiento.Value.Date > hoy.AddYears(-edad))
            {
                edad--;
            }

            return edad;
        }

        private static int BoolToInt(bool valor) => valor ? 1 : 0;

        private static string Dec(decimal valor) => valor.ToString(System.Globalization.CultureInfo.InvariantCulture);

        /// <summary>
        /// Borra un expediente. VB6 (sbBorrar, frmPreaEstudiov2.frm línea ~12087):
        ///   clsEntidad.tablaName = "spCRDPreaPREANALISIS"
        ///   clsEntidad.fxRemover(cod_preanalisis, cod_preanalisis_ref)
        ///   -&gt; EXEC spCRDPreaPREANALISIS_B '&lt;cod_preanalisis&gt;', &lt;cod_preanalisis_ref o Null&gt;
        /// Solo permitido si el expediente está en modo edición (VB6: m_ventanaEnModo =
        /// ModificarRegistro) — validación replicada en BL.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2GuardarResponse> Prea_frmPreaEstudiov2_Borrar(
            int codEmpresa, string codPreanalisis, string codPreanalisisRef)
        {
            var result = new ErrorDto<FrmPreaEstudiov2GuardarResponse>
            {
                Code = 0,
                Description = "Ok",
                Result = new FrmPreaEstudiov2GuardarResponse()
            };

            var expediente = (codPreanalisis ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(expediente))
            {
                result.Code = -1;
                result.Description = "Debe indicar el expediente a borrar.";
                return result;
            }

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);

                const string sql = "EXEC spCRDPreaPREANALISIS_B @Expediente, @ExpedienteRef";
                connection.Execute(sql, new
                {
                    Expediente = expediente,
                    ExpedienteRef = string.IsNullOrWhiteSpace(codPreanalisisRef)
                        ? null
                        : codPreanalisisRef.Trim()
                });

                result.Result.cod_preanalisis = expediente;
                result.Result.mensaje = "La información fue borrada correctamente.";
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }

            return result;
        }
    }
}
