using System.Data;
using System.Globalization;
using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public partial class FrmCrSeguimientoTramitesDb
    {
        /// <summary>
        /// Valida y aplica la formalización de la operación conservando el flujo de
        /// cmdAplicarFormalizacion_Click y sbFormalizar del formulario VB6.
        /// </summary>
        public ErrorDto<CrSeguimientoTramitesFormalizacionResult>
            Cr_SeguimientoTramites_Formalizacion_Aplicar(
                int codEmpresa,
                CrSeguimientoTramitesFormalizacionAplicarRequest request)
        {
            var result = new CrSeguimientoTramitesFormalizacionResult();
            string? mensajeRequest = Cr_SeguimientoTramites_Formalizacion_Request_Validar(request);
            if (!string.IsNullOrWhiteSpace(mensajeRequest))
            {
                return DbHelper.CreateErrorResponse(mensajeRequest, -2, result);
            }

            var globalesResp = _mainDb.sbSifParametrosInicializa(codEmpresa, request.usuario.Trim());
            if (globalesResp.Code != 0 || globalesResp.Result is null)
            {
                return DbHelper.CreateErrorResponse(
                    globalesResp.Description ?? "No fue posible obtener los parámetros globales.",
                    globalesResp.Code.GetValueOrDefault(-1),
                    result);
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
                conn.Open();
                DateTime fechaSistema = globalesResp.Result.fxFechaServidor ?? DateTime.Today;

                Cr_SeguimientoTramites_Formalizacion_PrevioValidar_Ejecutar(
                    conn,
                    request,
                    fechaSistema);

                List<string> mensajes = Cr_SeguimientoTramites_Formalizacion_Validar(
                    conn,
                    codEmpresa,
                    request,
                    fechaSistema,
                    globalesResp.Result.GlngFechaCR);

                if (mensajes.Count > 0)
                {
                    return DbHelper.CreateErrorResponse(
                        string.Join(Environment.NewLine, mensajes),
                        -2,
                        result);
                }

                CrSeguimientoTramitesFormalizacionAplicarRaw raw =
                    Cr_SeguimientoTramites_Formalizacion_Procedimiento_Ejecutar(
                        conn,
                        request,
                        globalesResp.Result.GOficinaTitular);

                if (raw.pasaformalizacion != 1)
                {
                    return DbHelper.CreateErrorResponse(
                        string.IsNullOrWhiteSpace(raw.errormsj)
                            ? "No fue posible aplicar la formalización."
                            : raw.errormsj.Trim(),
                        -2,
                        result);
                }

                result.operacion = request.operacion;
                result.aplicado = true;
                result.imprime_boleta_ck = raw.boletack == 1;
                result.mensaje = "Formalización Aplicada Satisfactoriamente...";

                Cr_SeguimientoTramites_Formalizacion_Bitacora_Registrar(
                    codEmpresa,
                    request.usuario,
                    $"Formalización de la OP: {request.operacion}");

                return DbHelper.CreateOkResponse(result, result.mensaje);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, result);
            }
        }

        /// <summary>
        /// Valida y anula la formalización del mes en curso conservando el flujo de
        /// fxVerificaAnulacion y sbAnular del formulario VB6.
        /// </summary>
        public ErrorDto<CrSeguimientoTramitesFormalizacionResult>
            Cr_SeguimientoTramites_Formalizacion_Anular(
                int codEmpresa,
                CrSeguimientoTramitesFormalizacionAnularRequest request)
        {
            var result = new CrSeguimientoTramitesFormalizacionResult();
            if (request is null
                || request.operacion <= 0
                || string.IsNullOrWhiteSpace(request.usuario))
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar la operación y el usuario.",
                    -2,
                    result);
            }

            var globalesResp = _mainDb.sbSifParametrosInicializa(codEmpresa, request.usuario.Trim());
            if (globalesResp.Code != 0 || globalesResp.Result is null)
            {
                return DbHelper.CreateErrorResponse(
                    globalesResp.Description ?? "No fue posible obtener los parámetros globales.",
                    globalesResp.Code.GetValueOrDefault(-1),
                    result);
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
                conn.Open();
                string mensajeProcedimiento;

                using (IDbTransaction transaction = conn.BeginTransaction())
                {
                    // El VB6 depura las refundiciones inconsistentes antes de validar la anulación.
                    Cr_SeguimientoTramites_Formalizacion_RefundicionesEspejo_Depurar(
                        conn,
                        transaction,
                        request.operacion);

                    List<string> mensajes = Cr_SeguimientoTramites_Formalizacion_Anulacion_Validar(
                        conn,
                        transaction,
                        request,
                        globalesResp.Result.SysPlanPagos);

                    if (mensajes.Count > 0)
                    {
                        transaction.Rollback();
                        return DbHelper.CreateErrorResponse(
                            string.Join(Environment.NewLine, mensajes),
                            -2,
                            result);
                    }

                    try
                    {
                        mensajeProcedimiento = conn.QueryFirstOrDefault<string>(
                            "exec spCRDFormalizaAnulacion @Operacion, @Usuario, 1;",
                            new
                            {
                                Operacion = request.operacion,
                                Usuario = request.usuario.Trim()
                            },
                            transaction) ?? string.Empty;

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }

                result.operacion = request.operacion;
                result.aplicado = true;
                result.imprime_recibo_anulacion = globalesResp.Result.SysDocVersion == 2;
                result.mensaje = string.Join(
                    Environment.NewLine,
                    new[] { mensajeProcedimiento.Trim(), "...Anulación Realizada Satisfactoriamente..." }
                        .Where(texto => !string.IsNullOrWhiteSpace(texto)));

                Cr_SeguimientoTramites_Formalizacion_Anulacion_Rastros_Registrar(
                    codEmpresa,
                    request);

                return DbHelper.CreateOkResponse(result, result.mensaje);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, result);
            }
        }

        /// <summary>
        /// Actualiza la fecha de formalización y la fecha de desembolso de la operación,
        /// equivalente a dtpFechaFormalizacion_Change e imgGuardaFecDesembolso_Click del VB6.
        /// </summary>
        public ErrorDto Cr_SeguimientoTramites_Formalizacion_Fechas_Actualizar(
            int codEmpresa,
            CrSeguimientoTramitesFormalizacionFechasRequest request)
        {
            if (request is null || request.operacion <= 0)
            {
                return DbHelper.ErrorResponse("Debe indicar la operación.", -2);
            }

            if (!request.fecha_formalizacion.HasValue && !request.fecha_desembolso.HasValue)
            {
                return DbHelper.ErrorResponse("Debe indicar al menos una fecha.", -2);
            }

            const string sql = """
                update reg_creditos
                set fechaforp = isnull(@FechaFormalizacion, fechaforp),
                    fecha_inicio_calculo = case
                        when @FechaDesembolso is not null and estadosol not in ('F', 'N')
                            then @FechaDesembolso
                        else fecha_inicio_calculo
                    end
                where id_solicitud = @Operacion;
                """;

            var response = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    FechaFormalizacion = request.fecha_formalizacion,
                    FechaDesembolso = request.fecha_desembolso,
                    Operacion = request.operacion
                });

            if (response.Code != 0)
            {
                return response;
            }

            Cr_SeguimientoTramites_Formalizacion_Bitacora_Registrar(
                codEmpresa,
                request.usuario,
                $"Fecha de Desembolso Operacion {request.operacion}",
                "Modifica - WEB");

            return DbHelper.OkResponse("Fecha de Desembolso Actualizada satisfactoriamente...");
        }

        /// <summary>
        /// Actualiza los indicadores de primer cuota y traslado de salario,
        /// equivalente a chkPrimera_Click y chkTrasladoSalario_Click del VB6.
        /// </summary>
        public ErrorDto Cr_SeguimientoTramites_Formalizacion_Indicadores_Actualizar(
            int codEmpresa,
            CrSeguimientoTramitesFormalizacionIndicadoresRequest request)
        {
            if (request is null || request.operacion <= 0)
            {
                return DbHelper.ErrorResponse("Debe indicar la operación.", -2);
            }

            const string sql = """
                update reg_creditos
                set PRIMER_CUOTA = @PrimerCuota,
                    IND_APLICA_TRASLADO_SALARIO = @TrasladoSalario
                where id_solicitud = @Operacion;
                """;

            return DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    PrimerCuota = request.ind_primera_cuota ? "S" : "N",
                    TrasladoSalario = request.ind_traslado_salario ? 1 : 0,
                    Operacion = request.operacion
                });
        }

        private static string? Cr_SeguimientoTramites_Formalizacion_Request_Validar(
            CrSeguimientoTramitesFormalizacionAplicarRequest? request)
        {
            if (request is null || request.operacion <= 0)
            {
                return "Debe indicar la operación.";
            }

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return "Debe indicar el usuario.";
            }

            if (string.IsNullOrWhiteSpace(request.codigo))
            {
                return "Debe indicar la línea de crédito.";
            }

            if (string.IsNullOrWhiteSpace(request.recurso))
            {
                return "Debe indicar el recurso.";
            }

            return null;
        }

        /// <summary>
        /// Ejecuta los ajustes que el VB6 realiza dentro de fxVerificaFormalizacion antes de validar:
        /// reubica la fecha de formalización al mes en curso y recalcula los cargos de hipotecas.
        /// </summary>
        private static void Cr_SeguimientoTramites_Formalizacion_PrevioValidar_Ejecutar(
            IDbConnection conn,
            CrSeguimientoTramitesFormalizacionAplicarRequest request,
            DateTime fechaSistema)
        {
            if (request.fecha_formalizacion.Month != fechaSistema.Month
                || request.fecha_formalizacion.Year != fechaSistema.Year)
            {
                conn.Execute(
                    "update reg_creditos set fechaforp = @Fecha where id_solicitud = @Operacion;",
                    new { Fecha = fechaSistema.Date, Operacion = request.operacion });

                request.fecha_formalizacion = fechaSistema.Date;
            }

            if (!string.Equals(request.garantia.Trim(), "H", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            conn.Execute(
                "exec spCRDOperacionCargosAdd @Operacion, @Codigo, @Monto;",
                new
                {
                    Operacion = request.operacion,
                    Codigo = request.codigo.Trim(),
                    Monto = request.monto
                });
        }

        private static CrSeguimientoTramitesFormalizacionAplicarRaw
            Cr_SeguimientoTramites_Formalizacion_Procedimiento_Ejecutar(
                IDbConnection conn,
                CrSeguimientoTramitesFormalizacionAplicarRequest request,
                string oficinaTitular)
        {
            var parameters = new
            {
                Operacion = request.operacion,
                DeducePlanilla = request.ind_deduce_planilla ? 1 : 0,
                Deductora = request.deductora_id,
                PriDeduc = Cr_SeguimientoTramites_Formalizacion_PrimerDeduccion_Componer(
                    request.primer_deduccion_anio,
                    request.primer_deduccion_mes,
                    request.primer_deduccion_quincena),
                FechaDesembolso = request.fecha_desembolso.ToString(
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture),
                Recurso = Cr_SeguimientoTramites_Filtro_Normalizar(request.recurso, 10),
                Oficina = Cr_SeguimientoTramites_Filtro_Normalizar(oficinaTitular, 10),
                TasaFacial = request.tasa_facial,
                Documento = Cr_SeguimientoTramites_Filtro_Normalizar(request.documento, 20),
                Usuario = Cr_SeguimientoTramites_Filtro_Normalizar(request.usuario, 30),
                EnviarTesoreria = request.ind_enviar_tesoreria ? 1 : 0,
                Pagare = request.pagare,
                DocumentoReferido = Cr_SeguimientoTramites_Filtro_Normalizar(
                    request.documento_referido,
                    18)
            };

            return conn.QueryFirst<CrSeguimientoTramitesFormalizacionAplicarRaw>(
                """
                exec spCrd_SGT_Formalizacion
                    @Operacion, @DeducePlanilla, @Deductora, @PriDeduc, @FechaDesembolso,
                    @Recurso, @Oficina, @TasaFacial, @Documento, @Usuario,
                    @EnviarTesoreria, @Pagare, @DocumentoReferido;
                """,
                parameters);
        }

        private static void Cr_SeguimientoTramites_Formalizacion_RefundicionesEspejo_Depurar(
            IDbConnection conn,
            IDbTransaction transaction,
            int operacion)
        {
            conn.Execute(
                """
                delete from refundiciones
                where id_solicitud = id_solicitudr and id_solicitud = @Operacion;
                """,
                new { Operacion = operacion },
                transaction);
        }

        private void Cr_SeguimientoTramites_Formalizacion_Anulacion_Rastros_Registrar(
            int codEmpresa,
            CrSeguimientoTramitesFormalizacionAnularRequest request)
        {
            Cr_SeguimientoTramites_Formalizacion_Bitacora_Registrar(
                codEmpresa,
                request.usuario,
                $"Anulación de la OP: {request.operacion}");

            MCredito.SbBitacoraCredito(
                _portalDb,
                codEmpresa,
                new MCredito.CrBitacoraCreditoRequest
                {
                    usuario = request.usuario,
                    tipo = "C",
                    movimiento = "13",
                    detalle = $"Monto : {request.monto.ToString("N2", CultureInfo.InvariantCulture)}",
                    operacion = request.operacion,
                    codigo = request.codigo,
                    notas = "SGT Anula Formalizacion del Día"
                });

            string operacionTexto = request.operacion.ToString(CultureInfo.InvariantCulture);
            _mainDb.sbTrazabilidad_Inserta(
                codEmpresa,
                "09",
                operacionTexto,
                operacionTexto,
                request.usuario.Trim());
        }

        private void Cr_SeguimientoTramites_Formalizacion_Bitacora_Registrar(
            int codEmpresa,
            string usuario,
            string detalle,
            string movimiento = "Registra - WEB")
        {
            _securityMainDb.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = (usuario ?? string.Empty).Trim(),
                Modulo = CrSeguimientoTramitesModulo,
                Movimiento = movimiento,
                DetalleMovimiento = detalle
            });
        }
    }
}
