using Galileo.BusinessLogic;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.KindoSinpe;
using Galileo.Models.ProGrX.Bancos;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX.Bancos;
using Microsoft.ReportingServices.ReportProcessing.ReportObjectModel;
using Sinpe_PIN;

namespace Galileo_API.DataBaseTier
{
    /// <summary>
    /// Validador y emisor SINPE para Coope San Gabriel (CSG).
    /// - Elimina duplicidad entre Crédito Directo (PIN) y Tiempo Real (DTR)
    /// - Centraliza pipeline: validar -> enviar -> persistir respuesta -> bitácora
    /// </summary>
    public sealed class CoopeSanGabrielValidator
    {
        private readonly PortalDB _portalDB;
        private readonly SinpeGalileoPin _sinpePIN;
        private readonly SinpeGalileoDtr _sinpeDTR;
        private readonly MKindoServiceDb _mKindo;
        private readonly MTesoreria _mTesoreria;

        private const string SinpeRejectionMessage = "Rechazo SINPE";

        public CoopeSanGabrielValidator(IConfiguration config)
        {
            _mKindo = new MKindoServiceDb(config);
            _sinpePIN = new SinpeGalileoPin(config);
            _sinpeDTR = new SinpeGalileoDtr(config);
            _mTesoreria = new MTesoreria(config);
            _portalDB = new PortalDB(config);
        }

        private enum SinpeTipo { PIN, TR }

        #region Validación de Solicitud SINPE

        public ErrorDto fxValidacionSinpe(int codEmpresa, string solicitud, string usuario, string? tipo = "PIN")
        {
            var ok = DbHelper.CreateOkResponse();

            try
            {
                var parametrosSinpe = _mKindo.GetUriEmpresa(codEmpresa, usuario);
                if (parametrosSinpe?.Result == null)
                    return DbHelper.ErrorResponse("No se pudieron obtener parámetros SINPE para la empresa.");

                var infoSinpeResult = CargarInfoSinpe(codEmpresa, solicitud);
                if (infoSinpeResult.Code == -1)
                    return DbHelper.ErrorResponse(infoSinpeResult.Description ?? "Error al consultar info SINPE.");

                var info = infoSinpeResult.Result!;
                if (!TieneDatosMinimos(info))
                    return ok; // mismo comportamiento actual: si no hay datos, regresa Ok

                if (!MKindoServiceDb.IsValidCostaRicaIBAN(info.CuentaIBAN!))
                    return DbHelper.ErrorResponse("Cuenta IBAN no válida");

                var sinpeTipo = ParseTipo(tipo);
                var context = CrearContexto(parametrosSinpe);

                var uriConn = GetServiceUri(parametrosSinpe, sinpeTipo);

                var servicio = tipo == "PIN"
                    ? _sinpePIN.IsServiceAvailable(uriConn, context)
                    : _sinpeDTR.IsServiceAvailable(uriConn, context);
                if (!servicio.ServiceAvailable)
                    return DbHelper.ErrorResponse(servicio.Errors?[0]?.Message ?? "Servicio no disponible");

                string cedula = MKindoServiceDb.MaskSinpeId(info.tipoID, info.Cedula!);

                var cuenta = ConsultarCuenta(parametrosSinpe, context, info.CuentaIBAN!, sinpeTipo, cedula);

                if (!cuenta.IsSuccessful)
                {
                    var err = cuenta.Errors;
                    if (err != null && err.Length > 0)
                    {
                        ok.Code = err[0].Code;
                        ok.Description = err[0].Message;
                    }
                    else
                    {
                        ok.Code = -1;
                        ok.Description = "Error desconocido al consultar la cuenta.";
                    }

                    return ok;
                }

                // Estados 0/1: OK; otros: rechazo con motivo
                var estado = (cuenta.Account?.State ?? 0);

                if(cedula.Replace("-", "") != cuenta.Account!.HolderId!.Replace("-", ""))
                {
                    return DbHelper.ErrorResponse("La cuenta IBAN no pertenece a la Cedula");
                }


                if (estado == 0 || estado == 1)
                {
                    var desc = $@"La cuenta IBAN {info.CuentaIBAN} registrada a
nombre de {cuenta.Account!.Holder} cédula: {cuenta.Account.HolderId} Tipo Id: {info.tipoID}
Tipo de Moneda: {cuenta.Account.CurrencyCode} Entidad: {cuenta.Account.EntityCode}-{cuenta.Account.EntityName}";

                    return DbHelper.OkResponse(desc);
                }
                else
                {
                    var rechazo = _mKindo.fxTesConsultaMotivo(codEmpresa, estado).Result ?? SinpeRejectionMessage;
                    return DbHelper.ErrorResponse(rechazo, estado);
                }
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse("Ocurrió un problema con la validación. - " + ex.Message);
            }
        }

        private ErrorDto<vInfoSinpe> CargarInfoSinpe(int codEmpresa, string solicitud)
        {
            return _mKindo.fxTesConsultaInfoSinpe(codEmpresa, solicitud);
        }

        private static bool TieneDatosMinimos(vInfoSinpe info) =>
            !string.IsNullOrWhiteSpace(info?.Cedula) &&
            !string.IsNullOrWhiteSpace(info!.CuentaIBAN);

        private static ReqBase CrearContexto(ErrorDto<ParametrosSinpe> parametrosSinpe) =>
            new ReqBase
            {
                HostId = parametrosSinpe.Result!.vHostPin,
                OperationId = Guid.NewGuid().ToString(),
                ClientIPAddress = parametrosSinpe.Result.vIpHost,
                CultureCode = "ES-CR",
                UserCode = parametrosSinpe.Result.vUsuarioLog,
                vCanalCGP = parametrosSinpe.Result.vCanalCGP
            };

        private Galileo.Models.KindoSinpe.ResAccountInfo ConsultarCuenta(
            ErrorDto<ParametrosSinpe> parametrosSinpe,
            ReqBase context,
            string cuentaIban,
            SinpeTipo tipo,
            string cedula)
        {
            var accountData = new Galileo.Models.KindoSinpe.ReqAccountInfo
            {
                HostId = context.HostId,
                OperationId = context.OperationId,
                ClientIPAddress = context.ClientIPAddress,
                CultureCode = context.CultureCode,
                UserCode = context.UserCode,
                Id = cedula,
                AccountNumber = cuentaIban
            };

            var uri = GetServiceUri(parametrosSinpe, tipo);
            // enum compare, no string compare
            if (tipo == SinpeTipo.PIN)
                return _sinpePIN.GetAccountInfo(uri, accountData);

            return _sinpeDTR.GetAccountInfo(uri, accountData);
        }

        private static SinpeTipo ParseTipo(string? tipo) =>
            string.Equals(tipo, "TR", StringComparison.OrdinalIgnoreCase)
                ? SinpeTipo.TR
                : SinpeTipo.PIN;

        private static string GetServiceUri(ErrorDto<ParametrosSinpe> parametros, SinpeTipo tipo)
        {
            var r = parametros.Result!;
            return tipo == SinpeTipo.PIN
                ? (r.UrlCGP_PIN ?? string.Empty)
                : (r.UrlCGP_DTR ?? string.Empty);
        }

        #endregion

        #region Emisión SINPE (PIN / TR) - pipeline unificado

        public ErrorDto fxTesEmisionSinpeCreditoDirecto(
            int codEmpresa,
            int nSolicitud,
            DateTime fecha,
            string usuario,
            int docBase,
            int contador)
        {
            Parametros parametros = new()
            {
                codEmpresa = codEmpresa,
                nSolicitud = nSolicitud,
                usuario = usuario,
                fecha = fecha
            };

            return ProcesarEmision(parametros,
                docBase, contador,
                SinpeTipo.PIN,
                enviar: (ce, ns, u) => EnviarPinCreditoDirecto(ce, ns, u),
                bitacoraExito: "Emisión Transferencia Sinpe: Exitosa",
                bitacoraRechazo: "Transferencia Sinpe rechazada");
        }

        public ErrorDto fxTesEmisionSinpeTiempoReal(
            int codEmpresa,
            int nSolicitud,
            DateTime fecha,
            string usuario,
            int docBase,
            int contador)
        {
            Parametros parametros = new()
            {
                codEmpresa = codEmpresa,
                nSolicitud = nSolicitud,
                usuario = usuario,
                fecha = fecha
            };
            return ProcesarEmision(
                parametros,
                docBase, contador,
                SinpeTipo.TR,
                enviar: (ce, ns, u) => EnviarTiempoRealDtr(ce, ns, u),
                bitacoraExito: "Emisión Transferencia Sinpe: Exitosa",
                bitacoraRechazo: "Transferencia Sinpe rechazada");
        }

        private ErrorDto ProcesarEmision(
            Parametros parametros,
            int docBase,
            int contador,
            SinpeTipo tipo,
            Func<int, int, string, ErrorDto<RespuestaRegistro>> enviar,
            string bitacoraExito,
            string bitacoraRechazo)
        {
            var response = new ErrorDto { Code = 0, Description = "Ok" };

            var datos = new TesTransaccion();
            bool estadoSinpe = true;
            int idRechazo = 0;
            string rechazoTexto = "";
            RespuestaRegistro? respuesta = null;

            try
            {
                if (parametros.nSolicitud <= 0)
                    return DbHelper.ErrorResponse("No se ha indicado una solicitud válida.");

                // 1) Validación disponibilidad / cuenta
                var servicioDisponible = fxValidacionSinpe(
                    parametros.codEmpresa,
                    parametros.nSolicitud.ToString(),
                    parametros.usuario!,
                    tipo == SinpeTipo.PIN ? "PIN" : "TR");

                if (servicioDisponible.Code != 0 && servicioDisponible.Code != 1)
                {
                    estadoSinpe = false;
                    idRechazo = servicioDisponible.Code ?? -1;

                    rechazoTexto = _mKindo.fxTesConsultaMotivo(parametros.codEmpresa, idRechazo).Result ?? SinpeRejectionMessage;
                    response = DbHelper.ErrorResponse($"N°: {parametros.nSolicitud} - {rechazoTexto}");

                    fxGuardaID_RespuestaSinpe(parametros.codEmpresa, idRechazo, parametros.nSolicitud.ToString());
                }
                // 2) Envío
                var envio = enviar(parametros.codEmpresa, parametros.nSolicitud, parametros.usuario!);
                respuesta = envio.Result;

                if (envio.Code != 0 || (respuesta != null && respuesta.MotivoError != 0))
                {
                    estadoSinpe = false;

                    idRechazo = respuesta?.MotivoError ?? envio.Code ?? -1;
                    rechazoTexto = _mKindo.fxTesConsultaMotivo(parametros.codEmpresa, idRechazo).Result ?? SinpeRejectionMessage;

                    response = DbHelper.ErrorResponse(rechazoTexto, idRechazo);
                }

                // 3) Persistir respuesta
                datos.NumeroSolicitud = parametros.nSolicitud;
                datos.FechaEmision = parametros.fecha;
                datos.FechaTraslado = parametros.fecha;
                datos.UsuarioGenera = parametros.usuario;
                datos.estadoSinpe = estadoSinpe;
                datos.IdMotivoRechazo = idRechazo;
                datos.CodigoReferencia = respuesta?.CodigoReferencia;
                datos.DocumentoBase = docBase.ToString();
                datos.contador = contador.ToString();

                if (!_mKindo.fxTesRespuestaSinpe(parametros.codEmpresa, datos).Result)
                {
                    _mTesoreria.sbTesBitacoraEspecial(
                        parametros.codEmpresa, parametros.nSolicitud, "10",
                        "Se produjo un error al actualizar la transacción",
                        parametros.usuario!);
                }

                // 4) Bitácora final
                _mTesoreria.sbTesBitacoraEspecial(
                    parametros.codEmpresa, parametros.nSolicitud, "10",
                    estadoSinpe ? bitacoraExito : $"{bitacoraRechazo}: {rechazoTexto}",
                    parametros.usuario!);

                return response;
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        #endregion

        #region Envío PIN / DTR (sin duplicidad de plantilla)

        private ErrorDto<RespuestaRegistro> EnviarPinCreditoDirecto(int codEmpresa, int nSolicitud, string usuario)
        {
            Parametros parametros = new()
            {
                codEmpresa = codEmpresa,
                nSolicitud = nSolicitud,
                usuario = usuario
            };
            return EnviarGenerico(
                parametros,
                tipo: SinpeTipo.PIN,
                buildRequest: (ctx, ce, sol, codRef) => BuildPinRequest(ctx, ce, sol, codRef),
                send: (uri, req) => _sinpePIN.SendPIN(uri, req),
                getSinpeRef: resp => resp.PINSendingResult?.SINPEReference ?? "",
                registrarCuenta: (ce, ns, resp) => _mKindo.RegistraCreditoCuenta(ce, ns, resp).Result);
        }

        private ErrorDto<RespuestaRegistro> EnviarTiempoRealDtr(int codEmpresa, int nSolicitud, string usuario)
        {
            Parametros parametros = new()
            {
                codEmpresa = codEmpresa,
                nSolicitud = nSolicitud,
                usuario = usuario
            };
            return EnviarGenerico(
                parametros,
                tipo: SinpeTipo.TR,
                buildRequest: (ctx, ce, sol, codRef) => BuildDtrRequest(ctx, ce, sol, codRef),
                send: (uri, req) => _sinpeDTR.SendDebit(uri, req),
                getSinpeRef: resp => resp.DTRSendingResult?.SINPERefNumber ?? "",
                registrarCuenta: (ce, ns, resp) => _mKindo.RegistraDibitoCuenta(ce, ns, resp).Result);
        }

        private ErrorDto<RespuestaRegistro> EnviarGenerico(
            Parametros parametros,
            SinpeTipo tipo,
            Func<ReqBase, int, dynamic, string, ReqSendingDynamic> buildRequest,
            Func<string, ReqSendingDynamic, dynamic> send,
            Func<dynamic, string> getSinpeRef,
            Func<int, int, dynamic, bool> registrarCuenta)
        {
            try
            {
                var parametrosSinpe = _mKindo.GetUriEmpresa(parametros.codEmpresa, parametros.usuario!);
                if (parametrosSinpe?.Result == null)
                    return new ErrorDto<RespuestaRegistro> { Code = -1, Description = "No se pudieron obtener parámetros SINPE." };

                var solicitud = _mKindo.fxTesConsultaSolicitud(parametros.codEmpresa, parametros.nSolicitud).Result;
                if (solicitud == null)
                    return new ErrorDto<RespuestaRegistro> { Code = -1, Description = "No se pudo consultar la solicitud." };

                var context = CrearContexto(parametrosSinpe);
                int canal = tipo switch
                {
                    SinpeTipo.PIN => 24,
                    SinpeTipo.TR => 1,
                    _ => 1
                };
                var codReferencia = string.IsNullOrWhiteSpace(solicitud.referencia_sinpe)
                    ? _mKindo.IsValidTransactionNumber(parametros.codEmpresa, canal)
                    : solicitud.referencia_sinpe;

                var req = buildRequest(context, parametros.codEmpresa, solicitud, codReferencia);

                var uri = GetServiceUri(parametrosSinpe, tipo);

                var resp = send(uri, req);

                var hasErrors = resp?.Errors?.Length > 0;

                // Manejo de errores del proveedor (guarda ID rechazo si viene)
                if (hasErrors)
                    fxGuardaID_RespuestaSinpe(parametros.codEmpresa, resp!.Errors[0].Code, parametros.nSolicitud.ToString(), codReferencia);

                if (resp == null || !resp!.IsSuccessful)
                {
                    var code = resp?.Errors != null && resp!.Errors.Length > 0 ? resp!.Errors[0].Code : -1;
                    var msg = resp?.Errors != null && resp!.Errors.Length > 0 ? resp!.Errors[0].Message : "Error al enviar solicitud a SINPE.";

                    // Movimientos en tránsito
                    _mKindo.RegistraMovTransito(parametros.codEmpresa, codReferencia, context.UserCode!, canal, resp, solicitud);

                    return new ErrorDto<RespuestaRegistro>
                    {
                        Code = -1,
                        Description = "Error al enviar solicitud",
                        Result = new RespuestaRegistro
                        {
                            MotivoError = code,
                            CodigoReferencia = "",
                            MotivoErrorInterno = msg
                        }
                    };
                }

                // Registrar respuesta en BD
                var actualizado = registrarCuenta(parametros.codEmpresa, parametros.nSolicitud, resp);
                if (!actualizado)
                {
                    return new ErrorDto<RespuestaRegistro>
                    {
                        Code = -1,
                        Description = "Error al actualizar el número de solicitud con la respuesta de SINPE.",
                        Result = new RespuestaRegistro { MotivoError = -1, CodigoReferencia = "" }
                    };
                }

                // Movimientos en tránsito
                _mKindo.RegistraMovTransito(parametros.codEmpresa, codReferencia, context.UserCode!, canal, resp, solicitud);

                return new ErrorDto<RespuestaRegistro>
                {
                    Code = 0,
                    Description = "Ok",
                    Result = new RespuestaRegistro
                    {
                        MotivoError = 0,
                        CodigoReferencia = getSinpeRef(resp)
                    }
                };
            }
            catch
            {
                return new ErrorDto<RespuestaRegistro>
                {
                    Code = -1,
                    Description = "Error al enviar la solicitud a SINPE.",
                    Result = null
                };
            }
        }

        #endregion

        #region Utilidades / Persistencia

        private void fxGuardaID_RespuestaSinpe(int codEmpresa, int codigo, string nSolicitud, string? referenciaSinpe = null)
        {
            const string qryExistSinpe = "SELECT COUNT(*) FROM SINPE_MOTIVOS WHERE cod_motivo = @codigo";
            var existe = DbHelper.ExecuteSingleQuery<int>(_portalDB, codEmpresa, qryExistSinpe, 0,new { codigo });
            if (existe.Result == 0) {
                //Agredo codigo de rechazo no existente en tabla de motivos SINPE para referencia
                const string insertMotivo = @"INSERT INTO SINPE_MOTIVOS (cod_motivo, descripcion, activo) VALUES (@codigo, @descripcion, 1)";
                DbHelper.ExecuteNonQuery(_portalDB, codEmpresa, insertMotivo, new { codigo, descripcion = $"Rechazo SINPE código {codigo}" });
            }


            const string query = @"UPDATE TES_TRANSACCIONES SET
                    ID_RECHAZO = @codigo, REFERENCIA_SINPE = COALESCE(@referenciaSinpe, REFERENCIA_SINPE)
                    WHERE NSOLICITUD = @nsolicitud";

            var parametros = new
            {
                codigo,
                referenciaSinpe,
                nsolicitud = nSolicitud
            };

            DbHelper.ExecuteNonQuery(_portalDB, codEmpresa, query, parametros);
        }

        public ErrorDto ConsultaCuentaSinpe(int codEmpresa, TesConsultaCuentaSinpeModels cuenta)
        {
            var parametrosSinpe = _mKindo.GetUriEmpresa(codEmpresa, cuenta.usuario);
            if (parametrosSinpe?.Result == null)
                return DbHelper.ErrorResponse("No se pudieron obtener parámetros SINPE.");

            var context = CrearContexto(parametrosSinpe);

            var servicio = _sinpePIN.IsServiceAvailable(parametrosSinpe.Result.UrlCGP_PIN!, context);
            if (!servicio.ServiceAvailable)
                return DbHelper.ErrorResponse(servicio.Errors?[0]?.Message ?? "Servicio no disponible");

            string cedula = MKindoServiceDb.MaskSinpeId(cuenta.tipoId, cuenta.cedula);

            var cuentaSinpe = ConsultarCuenta(parametrosSinpe, context, cuenta.cuentaIban, SinpeTipo.PIN, cedula);

            if (cuentaSinpe.Errors != null && cuentaSinpe.Errors.Length > 0)
            {
                var code = cuentaSinpe.Errors[0].Code;
                var rechazo = _mKindo.fxTesConsultaMotivo(codEmpresa, code).Result ?? SinpeRejectionMessage;
                return DbHelper.ErrorResponse(rechazo, code);
            }

            if (cuentaSinpe.Account?.State != null)
            {
                var estado = cuentaSinpe.Account.State!;
                var rechazo = _mKindo.fxTesConsultaMotivo(codEmpresa, estado ?? 0).Result ?? SinpeRejectionMessage;
                return DbHelper.ErrorResponse(rechazo, estado ?? 0);
            }

            return DbHelper.CreateOkResponse();
        }

        #endregion

        #region Construcción requests (PIN / DTR)

        private ReqSendingDynamic BuildPinRequest(ReqBase context, int codEmpresa, dynamic solicitud, string codReferencia)
        {
            var req = BuildBaseRequest(context);

            req.Transfer = new Galileo.Models.KindoSinpe.PINTransfer
            {
                ChannelReference = codReferencia,
                Amount = solicitud.Monto,
                CurrencyCode = MKindoServiceDb.GetCurrencyCodeDes(solicitud.Divisa!),
                Description = BuildDescription(solicitud),
                OriginEntityIBAN = solicitud.CuentaOrigen!,
                OriginCustomer = BuildOriginCustomer(codEmpresa, solicitud),
                DestinationCustomer = BuildDestinationCustomer(solicitud),
            };

            return req;
        }

        private ReqSendingDynamic BuildDtrRequest(ReqBase context, int codEmpresa, dynamic solicitud, string codReferencia)
        {
            var req = BuildBaseRequest(context);

            req.Debit = new DTR
            {
                ChannelRefNumber = codReferencia,
                Amount = solicitud.Monto,
                CurrencyCode = MKindoServiceDb.GetCurrencyCodeDes(solicitud.Divisa!),
                Description = BuildDescription(solicitud),
                OriginCustomer = BuildOriginCustomer(codEmpresa, solicitud),
                DestinationCustomer = BuildDestinationCustomer(solicitud),
            };

            return req;
        }

        private static ReqSendingDynamic BuildBaseRequest(ReqBase context)
        {
            return new ReqSendingDynamic
            {
                HostId = context.HostId,
                OperationId = context.OperationId,
                ClientIPAddress = context.ClientIPAddress,
                CultureCode = context.CultureCode,
                UserCode = context.UserCode,
                CoreIntegrationPoint = 1,
                CostCenter = 1,
                CustomData = BuildCustomData()
            };
        }

        private static string BuildDescription(dynamic s) =>
            $"{s.Detalle1}{s.Detalle2}{s.Detalle3}{s.Detalle4}";

        private Galileo.Models.KindoSinpe.OriginCustomer BuildOriginCustomer(int codEmpresa, dynamic s)
        {
            var ced = (s.CedulaOrigen as string ?? "").Replace("-", "");
            var info = MKindoServiceDb.Inferir(ced);

            return new Galileo.Models.KindoSinpe.OriginCustomer
            {
                Id = MKindoServiceDb.MaskSinpeId(Convert.ToInt32(info.Codigo), ced),
                IdType = Convert.ToInt32(info.Codigo),
                Name = s.NombreOrigen!,
                IBAN = s.CuentaOrigen!,
                DebitIBAN = _mKindo.fxSinpe_Valida_MovimientosPermitidos(codEmpresa, s.CuentaOrigen!),
                Email = (s.CorreoNotifica as string ?? "").Trim()
            };
        }

        private static Galileo.Models.KindoSinpe.DestinationCustomer BuildDestinationCustomer(dynamic s)
        {
            var ced = (s.Codigo as string ?? "").Replace("-", "");
            var info = MKindoServiceDb.Inferir(ced);

            return new Galileo.Models.KindoSinpe.DestinationCustomer
            {
                Id = MKindoServiceDb.MaskSinpeId(Convert.ToInt32(info.Codigo), ced),
                IdType = Convert.ToInt32(info.Codigo),
                Name = s.Beneficiario!,
                IBAN = s.Cuenta!,
                Email = s.CorreoNotifica
            };
        }

        private static List<Galileo.Models.KindoSinpe.CustomField> BuildCustomData() =>
            new() { new Galileo.Models.KindoSinpe.CustomField { Name = "Galileo", Value = "CSG" } };

        #endregion
    }
}
