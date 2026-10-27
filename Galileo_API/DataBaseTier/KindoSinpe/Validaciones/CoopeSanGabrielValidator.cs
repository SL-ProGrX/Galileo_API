using DocumentFormat.OpenXml.EMMA;
using Galileo.BusinessLogic;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.KindoSinpe;
using Galileo.Models.ProGrX.Bancos;
using Galileo.Models.Security;
using Galileo_API.Controllers.WFCSinpe;
using Galileo_API.Models.ProGrX.Bancos;
using Microsoft.ReportingServices.ReportProcessing.ReportObjectModel;
using Sinpe_PIN;
using static Galileo_API.DataBaseTier.CoopeSanGabrielValidator;

namespace Galileo_API.DataBaseTier
{
    /// <summary>
    /// Validador y emisor SINPE para Coope San Gabriel (CSG).
    /// - Elimina duplicidad entre Crédito Directo (PIN) y Tiempo Real (DTR)
    /// - Centraliza pipeline: validar -> enviar -> persistir respuesta -> bitácora
    /// </summary>
    public partial class CoopeSanGabrielValidator : IWfcSinpe
    {
        private readonly SinpeGalileoPin _sinpePIN;
        private readonly SinpeGalileoDtr _sinpeDTR;
        private readonly MKindoServiceDb _mKindo;
        private readonly MTesoreria _mTesoreria;
        private readonly bool _usaSimuladorKindo;
        private Galileo.Models.KindoSinpe.ParametrosSinpe _parametrosSinpe;

        public enum SinpeTipo { PIN, TR }

        private const string SinpeRejectionMessage = "Rechazo SINPE";

        // Cédula que devuelve el simulador de KINDO para cualquier cuenta (escenarios de pruebas, sección 1.2).
        private const string CedulaSimuladorKindo = "06-0378-0859";

        // Cache del "servicio disponible" durante la vida de la instancia (= un lote).
        // Evita repetir el round-trip remoto de disponibilidad por cada solicitud.
        private bool? _servicioDisponibleLote;

        public CoopeSanGabrielValidator(IConfiguration config)
        {
            _mKindo = new MKindoServiceDb(config);
            _sinpePIN = new SinpeGalileoPin(config);
            _sinpeDTR = new SinpeGalileoDtr(config);
            _mTesoreria = new MTesoreria(config);
            _usaSimuladorKindo = config["Sinpe:UsaSimuladorKindo"] == "true";
        }

        public ErrorDto fxValidacionSinpeTransaccion(int CodEmpresa, string cedula, string cuenta, string usuario)
        {
            ErrorDto response = new();
            _parametrosSinpe = _mKindo.GetUriEmpresa(CodEmpresa, usuario).Result ?? new Galileo.Models.KindoSinpe.ParametrosSinpe();
            var resp  = new ErrorDto<ParametrosSinpe>
            {
                Code = 0,
                Description = "OK",
                Result = _parametrosSinpe
            };

            if (_parametrosSinpe == null)
                return DbHelper.ErrorResponse("No se pudieron obtener parámetros SINPE para la empresa.");

            try
            {

                if (!string.IsNullOrEmpty(cedula) && !string.IsNullOrEmpty(cuenta))
                {
                    var context = CrearContexto(resp);

                    var uriConn = GetServiceUri(resp, SinpeTipo.PIN);

                    ErrorDto? errorDisponible = VerificarServicioDisponiblePIN(uriConn, context);

                    if (errorDisponible != null)
                        return errorDisponible;

                    var cuentaInfo = ConsultarCuenta(resp, context, cuenta, SinpeTipo.PIN, cedula);
                    response = ValidarRespuestaCuenta(CodEmpresa, cedula, cuentaInfo);
                }
                else
                {
                    response.Code = -1;
                    response.Description = " - Solicitud SINPE incompleta.";
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                _ = ex.Message; //bandera para visualizar error técnico.
                response.Description = "Ocurrió un problema con la validación.";
            }
            return response;
        }

        private ErrorDto ValidarRespuestaCuenta(int CodEmpresa, string cedula, Galileo.Models.KindoSinpe.ResAccountInfo cuentaInfo)
        {
            if (cuentaInfo.Account == null)
                return new ErrorDto
                {
                    Code = -1,
                    Description = cuentaInfo.Errors[0].Message
                };

            if (cuentaInfo.Account.AccountNumber == null)
            {
                var respustaError = _mKindo.fxTesConsultaMotivo(CodEmpresa, Convert.ToInt32(cuentaInfo.Account.State.ToString())).Result;
                return new ErrorDto
                {
                    Code = -1,
                    Description = respustaError
                };
            }

            if (NormalizarIdentificacion(cedula) != NormalizarIdentificacion(cuentaInfo.Account.HolderId!))
                return new ErrorDto
                {
                    Code = -1,
                    Description = "La cédula obtenida no corresponde con la cédula de la cuenta."
                };

            return new ErrorDto
            {
                Code = 0,
                Description = $@"La cuenta {cuentaInfo.Account.AccountNumber} registrada a
                                        nombre de {cuentaInfo.Account.Holder} cédula: {cuentaInfo.Account.HolderId} Tipo Id: 1 Tipo de Moneda: @moneda"
            };
        }

        private static string NormalizarIdentificacion(string valor) =>
            valor.Trim().Replace("-", "").Replace(" ", "").TrimStart('0');


        /// <summary>
        /// Creates a request context from SINPE parameters with host, user, channel, client IP, culture, and a new
        /// operation identifier.
        /// </summary>
        /// <param name="parametrosSinpe">Error wrapper containing the SINPE parameters used to populate the request context.</param>
        /// <returns>A <c>ReqBase</c> instance initialized with values from <c>parametrosSinpe.Result</c> and a newly generated
        /// operation identifier.</returns>
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

        private static string GetServiceUri(ErrorDto<ParametrosSinpe> parametros, SinpeTipo tipo)
        {
            var r = parametros.Result!;
            return tipo == SinpeTipo.PIN
                ? (r.UrlCGP_PIN ?? string.Empty)
                : (r.UrlCGP_DTR ?? string.Empty);
        }

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

        /// <summary>
        /// Verifica la disponibilidad del servicio SINPE una sola vez por lote (cachea el positivo).
        /// Devuelve un error si no está disponible, o null si está OK.
        /// </summary>
        private ErrorDto ValidarCuenta(int codEmpresa, string solicitud, string cedula, vInfoSinpe info, Galileo.Models.KindoSinpe.ResAccountInfo cuenta, ErrorDto ok)
        {
            if (cuenta.Account is null)
            {
                var mensaje = cuenta.Errors is { Length: > 0 } ? cuenta.Errors[0].Message : null;
                return DbHelper.ErrorResponse(mensaje ?? SinpeRejectionMessage);
            }

            // Igual que ASECCSS: solo la cuenta activa (estado 1 del servicio PIN, sección 10.8) puede recibir.
            if (cuenta.Account.State is not 1)
            {
                var motivo = _mKindo.fxTesConsultaMotivo(codEmpresa, cuenta.Account.State ?? 0).Result;
                return DbHelper.ErrorResponse(motivo ?? SinpeRejectionMessage);
            }

            var valOrigen = _mKindo.ValidaOrigenDestinoIBAN(codEmpresa, solicitud, cuenta.Account.CurrencyCode ?? "X");
            if (valOrigen.Code == -1)
                return DbHelper.ErrorResponse(valOrigen.Description ?? string.Empty);

            if (!cuenta.IsSuccessful)
            {
                var err = cuenta.Errors;
                if (err != null && err.Length > 0)
                {
                    ok.Code = err[0].Code;
                    ok.Description = err[0].Message;
                }
                return ok;
            }

            if (cuenta.Account == null || string.IsNullOrWhiteSpace(cuenta.Account.HolderId))
                return DbHelper.ErrorResponse("No se pudo validar el titular de la cuenta IBAN.");

            // Solo en ambiente de pruebas: el simulador devuelve una cédula fija para cualquier cuenta.
            if (_usaSimuladorKindo && cuenta.Account.HolderId == CedulaSimuladorKindo)
                cuenta.Account.HolderId = cedula;

            if (cedula.Replace("-", "") != cuenta.Account.HolderId.Replace("-", ""))
                return DbHelper.ErrorResponse("La cuenta IBAN no pertenece a la Cedula");

            ActualizaHolderIdEnCuenta(codEmpresa, solicitud, cuenta.Account.Holder);

            return ConstruirRespuestaEstado(codEmpresa, cuenta.Account.State.Value, info, cuenta);
        }

        private void ActualizaHolderIdEnCuenta(int codEmpresa, string solicitud, string holder)
        {
            _mKindo.fxTesActualizaHolderId(codEmpresa, solicitud, holder);
        }

        /// <summary>
        /// Construye la respuesta según el estado de la cuenta: 0/1 = OK con descripción; otros = rechazo con motivo.
        /// </summary>
        private ErrorDto ConstruirRespuestaEstado(
            int codEmpresa,
            int estado,
            vInfoSinpe info,
            Galileo.Models.KindoSinpe.ResAccountInfo cuenta)
        {
            if (estado == 1)
            {
                var account = cuenta.Account!;
                var desc = $@"La cuenta IBAN {info.CuentaIBAN} registrada a
nombre de {account.Holder} cédula: {account.HolderId} Tipo Id: {info.tipoID}
Tipo de Moneda: {account.CurrencyCode} Entidad: {account.EntityCode}-{account.EntityName}";

                return DbHelper.OkResponse(desc);
            }

            var rechazo = _mKindo.fxTesConsultaMotivo(codEmpresa, estado).Result ?? SinpeRejectionMessage;
            return DbHelper.ErrorResponse(rechazo, estado);
        }

        #region Operaciones de IWfcSinpe no implementadas para CSG (devuelven error explícito, nunca éxito)

        private static string MensajeNoImplementado(string operacion) =>
            $"{operacion} no está implementado para Coope San Gabriel.";

        public ErrorDto<bool> ServicioDisponible(string vUsuario) =>
            new ErrorDto<bool>
            {
                Code = -1,
                Description = MensajeNoImplementado("Consulta de disponibilidad por usuario"),
                Result = false
            };

        public ErrorDto<bool> GenerarFacturacionElectronica(int CodEmpresa,
            string pCedula,
            string pNumeroDocumento, string pTipoDoc,
            byte pTipoDocEletronico, string pNotas, string pTipoTramite) =>
            new ErrorDto<bool>
            {
                Code = -1,
                Description = MensajeNoImplementado("Facturación electrónica"),
                Result = false
            };

        public ErrorDto ConsultaCuentaSinpe(int CodEmpresa, TesConsultaCuentaSinpeModels cuenta) =>
            DbHelper.ErrorResponse(MensajeNoImplementado("Consulta de cuenta SINPE"));

        #endregion

        private static SinpeTipo ParseTipo(string? tipo) =>
            string.Equals(tipo, "TR", StringComparison.OrdinalIgnoreCase)
                ? SinpeTipo.TR
                : SinpeTipo.PIN;


        /// <summary>
        /// Verifica la disponibilidad del servicio SINPE una sola vez por lote (cachea el resultado),
        /// evitando repetir el round-trip remoto por cada solicitud.
        /// </summary>
        private bool ServicioDisponibleLote(int CodEmpresa, string vUsuario)
        {
            _servicioDisponibleLote ??= fxTesConsultarServicioDisponible(CodEmpresa, vUsuario).Result;
            return _servicioDisponibleLote.Value;
        }

        private ErrorDto<bool> fxTesConsultarServicioDisponible(int CodEmpresa, string vUsuario)
        {
            _parametrosSinpe = _mKindo.GetUriEmpresa(CodEmpresa, vUsuario).Result ?? new Galileo.Models.KindoSinpe.ParametrosSinpe();
            var resp = new ErrorDto<ParametrosSinpe>
            {
                Code = 0,
                Description = "OK",
                Result = _parametrosSinpe
            };

            var context = CrearContexto(resp);

            var uriConn = GetServiceUri(resp, SinpeTipo.PIN);

            ErrorDto? errorDisponible = VerificarServicioDisponiblePIN(uriConn, context);

            if (errorDisponible != null)
                return DbHelper.CreateErrorResponse<bool>(errorDisponible.Description ?? "Servicio SINPE no disponible.");

            return DbHelper.CreateOkResponse<bool>(true);

        }

    }

}
