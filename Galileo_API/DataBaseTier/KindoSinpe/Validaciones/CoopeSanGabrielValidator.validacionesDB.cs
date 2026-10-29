using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.KindoSinpe;
using Galileo_API.Models.ProGrX.Bancos;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Galileo_API.DataBaseTier
{
    /// <summary>
    /// Validación de Solicitud SINPE
    /// </summary>
    public partial class CoopeSanGabrielValidator
    {
        public ErrorDto fxValidacionSinpe(int CodEmpresa, string solicitud, string usuario, string? tipo = "PIN")
        {
            var ok = DbHelper.CreateOkResponse();
            try
            {
                ErrorDto<ParametrosSinpe> parametrosSinpe = _mKindo.GetUriEmpresa(CodEmpresa, usuario);

                _mKindo.UpdateOkTransaccionSinpe(CodEmpresa, string.Empty, 0, Convert.ToInt64(solicitud));

                if (parametrosSinpe?.Result == null)
                    return DbHelper.ErrorResponse("No se pudieron obtener parámetros SINPE para la empresa.");

                var infoSinpeResult = _mKindo.fxTesConsultaInfoSinpe(CodEmpresa, solicitud);
                if (infoSinpeResult.Code == -1)
                    return DbHelper.ErrorResponse(infoSinpeResult.Description ?? "Error al consultar info SINPE.");

                var info = infoSinpeResult.Result;
                if (info == null)
                    return DbHelper.ErrorResponse("No se pudo obtener información SINPE.");

                if (!MKindoServiceDb.IsValidCostaRicaIBAN(info.CuentaIBAN!))
                    return DbHelper.ErrorResponse("Cuenta IBAN no válida");

                var sinpeTipo = ParseTipo(tipo);
                var context = CrearContexto(parametrosSinpe);

                var uriConn = GetServiceUri(parametrosSinpe, sinpeTipo);

                ErrorDto? errorDisponible = null;
                if (sinpeTipo == SinpeTipo.PIN)
                    errorDisponible = VerificarServicioDisponiblePIN(uriConn, context);
                else
                    errorDisponible = VerificarServicioDisponibleDTR(uriConn, context);

                if (errorDisponible != null)
                    return errorDisponible;

                string cedula = MKindoServiceDb.MaskSinpeId(info.tipoID, info.Cedula!);

                var cuenta = ConsultarCuenta(parametrosSinpe, context, info.CuentaIBAN!, sinpeTipo, cedula);

                if (cuenta.Errors is { Length: > 0 })
                {
                    _mKindo.UpdateErrorTransaccionSinpe(
                                                CodEmpresa,
                                                string.Empty,
                                                cuenta.Errors[0].Code,
                                                Convert.ToInt64(solicitud)
                                               , usuario);
                }

                if (cuenta.Account != null)
                    ActualizaHolderIdEnCuenta(CodEmpresa, solicitud, cuenta.Account.Holder);

                return ValidarCuenta(CodEmpresa, solicitud, cedula, info, cuenta, ok);

            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse("Ocurrió un problema con la validación. - " + ex.Message);
            }
        }



    }
}
