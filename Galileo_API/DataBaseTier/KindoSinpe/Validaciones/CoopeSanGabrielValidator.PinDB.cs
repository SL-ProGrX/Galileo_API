using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.KindoSinpe;

namespace Galileo_API.DataBaseTier
{
    public partial class CoopeSanGabrielValidator
    {
        private ErrorDto? VerificarServicioDisponiblePIN(string uriConn, ReqBase context)
        {
            if (_servicioDisponibleLote == true)
                return null;

            var servicio = _sinpePIN.IsServiceAvailable(uriConn, context);
            if (!servicio.ServiceAvailable)
                return DbHelper.ErrorResponse("Servicio no disponible: " + servicio.Errors?[0].Message);

            _servicioDisponibleLote = true;
            return null;
        }

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
                enviar: (ce, ns, u) => EnviarTiempoRealDtr(ce, ns, u),
                bitacoraExito: "Emisión Transferencia Sinpe: Exitosa",
                bitacoraRechazo: "Transferencia Sinpe rechazada");
        }

        private ErrorDto ProcesarEmision(
           Parametros parametros,
           int docBase,
           int contador,
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
                    "PIN");

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

                if (envio.Result!.MotivoError == 32)
                {
                    // 3) Persistir respuesta
                    datos.NumeroSolicitud = parametros.nSolicitud;
                    datos.FechaEmision = parametros.fecha;
                    datos.FechaTraslado = parametros.fecha;
                    datos.UsuarioGenera = parametros.usuario;
                    datos.estadoSinpe = estadoSinpe;
                    datos.IdMotivoRechazo = envio.Result.MotivoError;
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
                }

                if (envio.Code != 0 || (respuesta != null && respuesta.MotivoError != 32))
                {
                    idRechazo = respuesta?.MotivoError ?? envio.Code ?? -1;
                    rechazoTexto = _mKindo.fxTesConsultaMotivo(parametros.codEmpresa, idRechazo).Result ?? SinpeRejectionMessage;

                    response = DbHelper.ErrorResponse(rechazoTexto, idRechazo);
                }


                return response;
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }
    }
}
