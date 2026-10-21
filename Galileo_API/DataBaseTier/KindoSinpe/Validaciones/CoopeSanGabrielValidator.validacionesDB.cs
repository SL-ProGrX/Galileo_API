using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.KindoSinpe;
using Galileo_API.Models.ProGrX.Bancos;

namespace Galileo_API.DataBaseTier
{
    /// <summary>
    /// Validación de Solicitud SINPE
    /// </summary>
    public partial class CoopeSanGabrielValidator
    {

      /// <summary>
      /// Validates SINPE information and account availability for a company request before continuing the process.
      /// </summary>
      /// <remarks>Validation includes configuration lookup, SINPE data retrieval, IBAN validation, service
      /// availability checks, account lookup, and final account validation.</remarks>
      /// <param name="codEmpresa">Company identifier used to load SINPE configuration and validations.</param>
      /// <param name="solicitud">Request identifier used to obtain SINPE data to validate.</param>
      /// <param name="usuario">User identifier used to resolve company-specific SINPE service settings.</param>
      /// <param name="tipo">SINPE operation type to validate, such as PIN or DTR. Defaults to PIN.</param>
      /// <returns>An <c>ErrorDto</c> with the validation result: success when validation passes (or when minimum SINPE data is
      /// not present), or an error response when any validation step fails or an exception occurs.</returns>
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

                var info = infoSinpeResult.Result;
                if (info == null)
                    return DbHelper.ErrorResponse("No se pudo obtener información SINPE.");

                if (!TieneDatosMinimos(info))
                    return ok; // mismo comportamiento actual: si no hay datos, regresa Ok

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

                return ValidarCuenta(codEmpresa, solicitud, cedula, info, cuenta, ok);
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse("Ocurrió un problema con la validación. - " + ex.Message);
            }
        }

        private static bool TieneDatosMinimos(vInfoSinpe info) =>
         !string.IsNullOrWhiteSpace(info?.Cedula) &&
         !string.IsNullOrWhiteSpace(info.CuentaIBAN);

        private static SinpeTipo ParseTipo(string? tipo) =>
            string.Equals(tipo, "TR", StringComparison.OrdinalIgnoreCase)
                ? SinpeTipo.TR
                : SinpeTipo.PIN;
        //TODO: pendiente de resolver
        public ErrorDto fxValidacionSinpeTransaccion(int CodEmpresa, string cedula, string cuenta, string usuario)
        {
            try
            {
                var parametrosSinpe = _mKindo.GetUriEmpresa(CodEmpresa, usuario);
                if (parametrosSinpe?.Result == null)
                    return DbHelper.ErrorResponse("No se pudieron obtener parámetros SINPE para la empresa.");


                return DbHelper.OkResponse("Validación de transacción no implementada para Coope San Gabriel.");

            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse("Ocurrió un problema con la validación. - " + ex.Message);
            }
        }

        public ErrorDto ConsultaCuentaSinpe(int codEmpresa, TesConsultaCuentaSinpeModels cuenta)
        {
            var parametrosSinpe = _mKindo.GetUriEmpresa(codEmpresa, cuenta.usuario);
            if (parametrosSinpe?.Result == null)
                return DbHelper.ErrorResponse("No se pudieron obtener parámetros SINPE.");

            var context = CrearContexto(parametrosSinpe);

            var servicio = _sinpePIN.IsServiceAvailable(parametrosSinpe.Result.UrlCGP_PIN ?? string.Empty, context);
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
                var estado = cuentaSinpe.Account.State;
                var rechazo = _mKindo.fxTesConsultaMotivo(codEmpresa, estado ?? 0).Result ?? SinpeRejectionMessage;
                return DbHelper.ErrorResponse(rechazo, estado ?? 0);
            }

            return DbHelper.CreateOkResponse();
        }
    }
}
