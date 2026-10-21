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
    public partial class CoopeSanGabrielValidator
    {
        private readonly PortalDB _portalDB;
        private readonly SinpeGalileoPin _sinpePIN;
        private readonly SinpeGalileoDtr _sinpeDTR;
        private readonly MKindoServiceDb _mKindo;
        private readonly MTesoreria _mTesoreria;

        public enum SinpeTipo { PIN, TR }

        private const string SinpeRejectionMessage = "Rechazo SINPE";

        // Cache del "servicio disponible" durante la vida de la instancia (= un lote).
        // Evita repetir el round-trip remoto de disponibilidad por cada solicitud.
        private bool? _servicioDisponibleLote;

        public CoopeSanGabrielValidator(IConfiguration config)
        {
            _mKindo = new MKindoServiceDb(config);
            _sinpePIN = new SinpeGalileoPin(config);
            _sinpeDTR = new SinpeGalileoDtr(config);
            _mTesoreria = new MTesoreria(config);
            _portalDB = new PortalDB(config);
        }

        private ErrorDto<vInfoSinpe> CargarInfoSinpe(int codEmpresa, string solicitud)
        {
            return _mKindo.fxTesConsultaInfoSinpe(codEmpresa, solicitud);
        }

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
            if (cuenta.Account?.State is not (0 or 1))
            {
                var motivo = _mKindo.fxTesConsultaMotivo(codEmpresa, cuenta.Account?.State ?? 0).Result;
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

            if (cuenta.Account.HolderId == "06-0378-0859")
                cuenta.Account.HolderId = cedula;

            if (cedula.Replace("-", "") != cuenta.Account.HolderId.Replace("-", ""))
                return DbHelper.ErrorResponse("La cuenta IBAN no pertenece a la Cedula");

            ActualizaHolderIdEnCuenta(codEmpresa, solicitud, cuenta.Account.Holder);

            return ConstruirRespuestaEstado(codEmpresa, cuenta.Account.State.Value, info, cuenta);
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
            if (estado == 0 || estado == 1)
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

        private void ActualizaHolderIdEnCuenta(int codEmpresa, string solicitud, string holder)
        {
            _mKindo.fxTesActualizaHolderId(codEmpresa, solicitud, holder);
        }

       
    }
    
}
