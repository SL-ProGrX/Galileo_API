using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.KindoSinpe;

namespace Galileo_API.DataBaseTier
{
    public partial class CoopeSanGabrielValidator
    {
        private ErrorDto? VerificarServicioDisponibleDTR(string uriConn, ReqBase context)
        {
            if (_servicioDisponibleLote == true)
                return null;

            var servicio = _sinpeDTR.IsServiceAvailable(uriConn, context);
            if (!servicio.ServiceAvailable)
                return DbHelper.ErrorResponse("Servicio no disponible: " + servicio.Errors?[0].Message);

            _servicioDisponibleLote = true;
            return null;
        }

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
                send: (uri, req) => _sinpePIN.SendPIN(uri, req),
                registrarCuenta: (ce, ns, resp) => _mKindo.RegistraDibitoCuenta(ce, ns, resp).Result);
        }

        private ErrorDto<RespuestaRegistro> EnviarGenerico(
            Parametros parametros,
            SinpeTipo tipo,
            Func<ReqBase, int, dynamic, string, ReqSendingDynamic> buildRequest,
            Func<string, ReqSendingDynamic, dynamic> send,
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
                    SinpeTipo.PIN => parametrosSinpe.Result.vCanalCGP,
                    SinpeTipo.TR => 24,
                    _ => 1
                };
                var codReferencia = string.IsNullOrWhiteSpace(solicitud.referencia_sinpe)
                    ? _mKindo.IsValidTransactionNumber(parametros.codEmpresa, canal)
                    : solicitud.referencia_sinpe;

                var req = buildRequest(context, parametros.codEmpresa, solicitud, codReferencia);

                var uri = GetServiceUri(parametrosSinpe, tipo);

                var resp = send(uri, req);

                var errorRespuesta = ProcesarRespuestaProveedor(parametros, context, solicitud, resp, canal, codReferencia);
                if (errorRespuesta != null)
                {
                    return new ErrorDto<RespuestaRegistro>
                    {
                        Code = -1,
                        Description = errorRespuesta.Result.MotivoErrorInterno,
                        Result = new RespuestaRegistro { MotivoError = -1, CodigoReferencia = resp }
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
                        Result = new RespuestaRegistro { MotivoError = -1, CodigoReferencia = resp }
                    };
                }

                // Movimientos en tránsito
                _mKindo.RegistraMovTransito(parametros.codEmpresa, codReferencia, context.UserCode, canal, resp, solicitud);

                return new ErrorDto<RespuestaRegistro>
                {
                    Code = 0,
                    Description = "Ok",
                    Result = new RespuestaRegistro
                    {
                        MotivoError = resp.PINSendingResult.State,
                        CodigoReferencia = resp.PINSendingResult.SINPERefNumber
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
    }
}
