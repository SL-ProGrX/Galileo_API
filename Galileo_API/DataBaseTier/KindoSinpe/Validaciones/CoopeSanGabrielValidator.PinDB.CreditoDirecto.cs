using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.KindoSinpe;
using Galileo.Models.Security;

namespace Galileo_API.DataBaseTier
{
    public partial class CoopeSanGabrielValidator
    {
        public ErrorDto fxTesEmisionSinpeCreditoDirectoTemp(
           int codEmpresa,
           int nSolicitud,
           DateTime fecha,
           string usuario,
           int docBase,
           int contador)
        {

            var response = new ErrorDto { Code = 0, Description = "Ok" };

            Parametros parametros = new()
            {
                codEmpresa = codEmpresa,
                nSolicitud = nSolicitud,
                usuario = usuario,
                fecha = fecha
            };

            var parametrosSinpeResponse = _mKindo.GetUriEmpresa(codEmpresa, usuario);
            if (parametrosSinpeResponse.Code != 0 || parametrosSinpeResponse.Result is null)
            {
                response.Code = parametrosSinpeResponse.Code.GetValueOrDefault(-1);
                if (response.Code == 0)
                    response.Code = -1;
                response.Description = parametrosSinpeResponse.Description ?? "No fue posible obtener la configuración SINPE de la empresa.";
                return response;
            }

            try
            {
                if (parametros.nSolicitud <= 0)
                    return DbHelper.ErrorResponse("No se ha indicado una solicitud válida.");

                // 1) Validación disponibilidad / cuenta
                var servicioDisponible = fxValidacionSinpe(
                    parametros.codEmpresa,
                    parametros.nSolicitud.ToString(),
                    parametros.usuario,
                    "PIN");

                if (servicioDisponible.Code != 0 && servicioDisponible.Code != 1)
                {
                    int idRechazo = servicioDisponible.Code ?? -1;

                    string rechazoTexto = _mKindo.fxTesConsultaMotivo(parametros.codEmpresa, idRechazo).Result ?? SinpeRejectionMessage;
                    response = DbHelper.ErrorResponse($"N°: {parametros.nSolicitud} - {rechazoTexto}");
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
