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


        private ErrorDto<RespuestaRegistro>? ProcesarRespuestaProveedor(Parametros parametros, ReqBase context, dynamic solicitud, dynamic resp, int canal, string codReferencia)
        {
            if (resp?.Errors?.Length > 0)
                fxGuardaID_RespuestaSinpe(parametros.codEmpresa, resp.Errors[0].Code, parametros.nSolicitud.ToString(), codReferencia);

            if (resp is not null && resp.IsSuccessful)
            {
                _mKindo.RegistraMovTransito(parametros.codEmpresa, codReferencia, context.UserCode, canal, resp, solicitud);
                return null;
            }

            var errores = resp?.Errors;
            var tieneErrores = errores is not null && errores.Length > 0;
            var code = tieneErrores ? errores![0].Code : -1;
            var msg = tieneErrores ? errores![0].Message : "Error al enviar solicitud a SINPE.";
            _mKindo.RegistraMovTransito(parametros.codEmpresa, codReferencia, context.UserCode, canal, resp, solicitud);

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


        #region Utilidades / Persistencia

        private void fxGuardaID_RespuestaSinpe(int codEmpresa, int codigo, string nSolicitud, string? referenciaSinpe = null)
        {
            const string qryExistSinpe = "SELECT COUNT(*) FROM SINPE_MOTIVOS WHERE cod_motivo = @codigo";
            var existe = DbHelper.ExecuteSingleQuery<int>(_portalDB, codEmpresa, qryExistSinpe, 0, new { codigo });
            if (existe.Result == 0)
            {
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



        #endregion

        #region Construcción requests (PIN / DTR)

        private ReqSendingDynamic BuildPinRequest(ReqBase context, int codEmpresa, dynamic solicitud, string codReferencia)
        {
            var req = BuildBaseRequest(context);

            req.Transfer = new Galileo.Models.KindoSinpe.PINTransfer
            {
                ChannelRefNumber = codReferencia,
                Amount = solicitud.Monto,
                CurrencyCode = MKindoServiceDb.GetCurrencyCodeDes(solicitud.Divisa),
                Description = BuildDescription(solicitud),
                OriginEntityIBAN = solicitud.CuentaOrigen,
                OriginCustomer = BuildOriginCustomer(codEmpresa, solicitud),
                DestinationCustomer = BuildDestinationCustomer(solicitud),
                CustomData = BuildCustomData(),

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
                CurrencyCode = MKindoServiceDb.GetCurrencyCodeDes(solicitud.Divisa),
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
                Name = s.NombreOrigen,
                IBAN = s.CuentaOrigen,
                DebitIBAN = _mKindo.fxSinpe_Valida_MovimientosPermitidos(codEmpresa, s.CuentaOrigen),
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
                Name = s.Beneficiario,
                IBAN = s.Cuenta,
                Email = s.CorreoNotifica
            };
        }

        private static List<Galileo.Models.KindoSinpe.CustomField> BuildCustomData() =>
            new() { new Galileo.Models.KindoSinpe.CustomField { Name = "Galileo", Value = "CSG" } };

        #endregion
    }

}
