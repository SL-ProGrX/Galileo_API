using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.KindoSinpe;

namespace Galileo_API.DataBaseTier
{
    public partial class CoopeSanGabrielValidator
    {
        private const int IdRechazoServicioNoDisponible = 83;
        private const int EstadoPINRegistrada = 1;
        private const int EstadoPINPagada = 32;
        private const int EstadoPINRechazada = 128;
        private const int EstadoPINEnEspera = 256;
        private const int MaxConsultasEstadoPIN = 3;
        private const int EsperaConsultaEstadoMs = 2000;
        private const int EstadoTransitoProcesado = 2;
        private const int EstadoTransitoError = 3;
        private const int RechazoEnvioNoConfirmado = -1;
        private const string MensajeConfiguracionSinpe = "No fue posible obtener la configuración SINPE de la empresa.";
        private const string MensajeEnvioNoConfirmado = "No fue posible enviar la transferencia SINPE. Consulte el estado antes de reintentar.";

        /// <summary>
        /// Datos de una emisión PIN ya preparada: se usa como referencia del canal en SINPE_MOV_TRANSITO y en KINDO.
        /// </summary>
        private sealed record EnvioPin(
            int CodEmpresa,
            int Nsolicitud,
            DateTime VFecha,
            string Usuario,
            int Contador,
            string? DocumentoBase,
            string ChannelRef);

        public ErrorDto fxTesEmisionSinpeCreditoDirecto(int CodEmpresa,
            int Nsolicitud, DateTime vfecha, string vUsuario, int doc_base, int contador)
        {
            return EjecutarEmisionPIN(CodEmpresa, Nsolicitud, vfecha, vUsuario, contador, null);
        }

        /// <summary>
        /// Flujo compartido de emisión por PIN (Crédito Directo y Tiempo Real).
        /// </summary>
        /// <param name="documentoBase">Documento base a guardar; si es null se usa el CBTrxNumber de KINDO.</param>
        private ErrorDto EjecutarEmisionPIN(int CodEmpresa, int Nsolicitud, DateTime vfecha, string vUsuario, int contador, string? documentoBase)
        {
            var parametrosSinpeResponse = _mKindo.GetUriEmpresa(CodEmpresa, vUsuario);
            if (parametrosSinpeResponse.Code != 0 || parametrosSinpeResponse.Result is null)
                return DbHelper.ErrorResponse(parametrosSinpeResponse.Description ?? MensajeConfiguracionSinpe);

            _parametrosSinpe = parametrosSinpeResponse.Result;

            if (Nsolicitud <= 0)
                return DbHelper.ErrorResponse("No se ha indicado una solicitud válida.");

            EnvioPin? envio = null;
            try
            {
                var solicitud = _mKindo.fxTesConsultaSolicitud(CodEmpresa, Nsolicitud);
                if (solicitud.Code != 0 || solicitud.Result is null)
                    return DbHelper.ErrorResponse(solicitud.Description ?? "No fue posible consultar la solicitud.");

                var channelRef = _mKindo.GenerarChannelRef(CodEmpresa, _parametrosSinpe.vCanalCGP);
                envio = new EnvioPin(CodEmpresa, Nsolicitud, vfecha, vUsuario, contador, documentoBase, channelRef);
                _mKindo.RegistrarTransitoPendiente(CodEmpresa, envio.ChannelRef, vUsuario, _parametrosSinpe.vCanalCGP, solicitud.Result);

                if (!ServicioDisponibleLote(CodEmpresa, vUsuario))
                {
                    var motivo = _mKindo.fxTesConsultaMotivo(CodEmpresa, IdRechazoServicioNoDisponible).Result ?? SinpeRejectionMessage;
                    return RechazarTransferencia(envio, IdRechazoServicioNoDisponible, motivo);
                }

                return EmitirTransferenciaPIN(envio, solicitud.Result);
            }
            catch (Exception ex)
            {
                return envio is null
                    ? DbHelper.ErrorResponse(ex.Message)
                    : RegistrarErrorEnvio(envio, ex.Message);
            }
        }

        private ErrorDto EmitirTransferenciaPIN(EnvioPin envio, TesTransaccion solicitud)
        {
            var respuesta = _sinpePIN.SendPIN(_parametrosSinpe.UrlCGP_PIN ?? string.Empty, ConstruirSolicitudPIN(solicitud, envio.ChannelRef));
            var resultadoEnvio = respuesta.PINSendingResult;
            if (!respuesta.IsSuccessful || resultadoEnvio is null)
            {
                var error = respuesta.Errors is { Length: > 0 } ? respuesta.Errors[0] : null;
                return RegistrarErrorEnvio(envio, error?.Message, error?.Code ?? RechazoEnvioNoConfirmado);
            }

            // KINDO aceptó la transferencia: queda en proceso hasta tener un estado final.
            _mKindo.ActualizarEstadoMovTransito(envio.CodEmpresa, envio.ChannelRef, resultadoEnvio.SINPERefNumber ?? string.Empty, EstadoTransitoProcesado, 0, string.Empty);

            var resultado = ConsultarEstadoFinal(envio.ChannelRef, resultadoEnvio);
            return resultado.State == EstadoPINPagada
                ? RegistrarExito(envio, resultado)
                : RegistrarNoExitoso(envio, resultado);
        }

        /// <summary>
        /// Re-consulta con GetTransferResult mientras la transferencia esté en un estado no final (sección 10.9).
        /// </summary>
        private PINSendingResult ConsultarEstadoFinal(string channelRef, PINSendingResult resultado)
        {
            for (var intento = 1; intento < MaxConsultasEstadoPIN && EsEstadoPendiente(resultado.State); intento++)
            {
                Thread.Sleep(EsperaConsultaEstadoMs);
                resultado = ConsultarResultadoPIN(channelRef) ?? resultado;
            }

            return resultado;
        }

        private PINSendingResult? ConsultarResultadoPIN(string channelRef)
        {
            var consulta = ContextoPIN(new ReqTransferInfoChannelRef { ChannelReference = channelRef });
            return _sinpePIN.GetPINResult(_parametrosSinpe.UrlCGP_PIN ?? string.Empty, consulta).PINSendingResult;
        }

        private static bool EsEstadoPendiente(short? estado) =>
            estado is EstadoPINEnEspera or EstadoPINRegistrada;

        private ErrorDto RegistrarExito(EnvioPin envio, PINSendingResult resultado)
        {
            _mKindo.ActualizarEstadoMovTransito(envio.CodEmpresa, envio.ChannelRef, resultado.SINPERefNumber ?? string.Empty, EstadoTransitoProcesado, 0, string.Empty);

            var datos = new TesTransaccion
            {
                NumeroSolicitud = envio.Nsolicitud,
                FechaEmision = envio.VFecha,
                FechaTraslado = envio.VFecha,
                UsuarioGenera = envio.Usuario,
                estadoSinpe = true,
                IdMotivoRechazo = 0,
                CodigoReferencia = resultado.SINPERefNumber,
                DocumentoBase = envio.DocumentoBase ?? resultado.CBTrxNumber,
                contador = envio.Contador.ToString()
            };

            if (!_mKindo.fxTesRespuestaSinpe(envio.CodEmpresa, datos).Result)
                _mTesoreria.sbTesBitacoraEspecial(envio.CodEmpresa, envio.Nsolicitud, "10", "Se produjo un error al actualizar la transacción", envio.Usuario);

            _mTesoreria.sbTesBitacoraEspecial(envio.CodEmpresa, envio.Nsolicitud, "10", "Emisión Transferencia Sinpe: Exitosa", envio.Usuario);
            return new ErrorDto { Code = 0, Description = "Ok" };
        }

        private ErrorDto RegistrarNoExitoso(EnvioPin envio, PINSendingResult resultado)
        {
            var idRechazo = resultado.State ?? EstadoPINEnEspera;
            var motivo = MensajeEstadoNoExitoso(idRechazo);

            _mKindo.ActualizarEstadoMovTransito(envio.CodEmpresa, envio.ChannelRef, resultado.SINPERefNumber ?? string.Empty, EstadoTransitoError, idRechazo, motivo);
            _mTesoreria.sbTesBitacoraEspecial(envio.CodEmpresa, envio.Nsolicitud, "10", $"Transferencia Sinpe rechazada: {motivo} Ref. canal: {envio.ChannelRef}", envio.Usuario);
            return RegistrarRechazo(envio.CodEmpresa, envio.Nsolicitud, envio.Usuario, idRechazo, motivo, resultado.SINPERefNumber ?? envio.ChannelRef);
        }

        private ErrorDto RechazarTransferencia(EnvioPin envio, int idRechazo, string motivo)
        {
            _mKindo.ActualizarEstadoMovTransito(envio.CodEmpresa, envio.ChannelRef, string.Empty, EstadoTransitoError, idRechazo, motivo);
            return RegistrarRechazo(envio.CodEmpresa, envio.Nsolicitud, envio.Usuario, idRechazo, motivo, envio.ChannelRef);
        }

        /// <summary>
        /// Error sin respuesta de KINDO (envío fallido o excepción): guarda ID_RECHAZO y REFERENCIA_SINPE (ChannelRef) en TES_TRANSACCIONES.
        /// </summary>
        private ErrorDto RegistrarErrorEnvio(EnvioPin envio, string? detalle, int idRechazo = RechazoEnvioNoConfirmado)
        {
            var descripcion = detalle ?? MensajeEnvioNoConfirmado;
            _mTesoreria.sbTesBitacoraEspecial(envio.CodEmpresa, envio.Nsolicitud, "10", $"Error en la transferencia Sinpe. Ref. canal: {envio.ChannelRef}. {descripcion}", envio.Usuario);
            _mKindo.ActualizarEstadoMovTransito(envio.CodEmpresa, envio.ChannelRef, string.Empty, EstadoTransitoError, idRechazo, descripcion);
            return RegistrarRechazo(envio.CodEmpresa, envio.Nsolicitud, envio.Usuario, idRechazo, MensajeEnvioNoConfirmado, envio.ChannelRef);
        }

        private ErrorDto RegistrarRechazo(int CodEmpresa, int Nsolicitud, string vUsuario, int idRechazo, string motivo, string referencia)
        {
            _mKindo.UpdateErrorTransaccionSinpe(CodEmpresa, referencia, idRechazo, Nsolicitud, vUsuario);
            return TesEmisionDocumentosSinpeResultado.TES_EmisionDocumentos_Sinpe_CrearRechazo(idRechazo, motivo);
        }

        private static string MensajeEstadoNoExitoso(int estado) => estado switch
        {
            EstadoPINRechazada => "Se ha rechazado la transacción. Favor intente más tarde.",
            EstadoPINEnEspera => "La confirmación de la transacción está en espera; consulte sus movimientos más tarde.",
            EstadoPINRegistrada => "La transferencia está registrada y pendiente de aprobación.",
            _ => "SINPE devolvió un estado de transferencia no exitoso."
        };

        /// <summary>
        /// Depósito PIN a otra entidad: la cuenta de origen es la interna del cliente (se debita)
        /// y la cuenta destino es la externa. OriginEntityIBAN y los campos opcionales vacíos se omiten.
        /// </summary>
        private ReqSendingDynamic ConstruirSolicitudPIN(TesTransaccion solicitud, string channelRef)
        {
            var origen = IdentificacionSinpe(solicitud.CedulaOrigen);
            var destino = IdentificacionSinpe(solicitud.Codigo);

            return ContextoPIN(new ReqSendingDynamic
            {
                CoreIntegrationPoint = 1,
                CostCenter = 1,
                Transfer = new PINTransfer
                {
                    ChannelRefNumber = channelRef,
                    Amount = Math.Round(solicitud.Monto, 4),
                    CurrencyCode = solicitud.Divisa == "DOL" ? E_CodigoMonedaISO4217.Dolares : E_CodigoMonedaISO4217.Colones,
                    Description = ConstruirDescripcion(solicitud),
                    OriginCustomer = new OriginCustomer
                    {
                        Id = origen.Id,
                        IdType = origen.Tipo,
                        Name = solicitud.NombreOrigen,
                        IBAN = solicitud.CuentaOrigen,
                        DebitIBAN = true
                    },
                    DestinationCustomer = new DestinationCustomer
                    {
                        Id = destino.Id,
                        IdType = destino.Tipo,
                        Name = solicitud.Beneficiario,
                        IBAN = solicitud.Cuenta,
                        Email = solicitud.CorreoNotifica
                    }
                }
            });
        }

        private T ContextoPIN<T>(T solicitud) where T : ReqBase
        {
            solicitud.HostId = _parametrosSinpe.vHostPin;
            solicitud.OperationId = Guid.NewGuid().ToString();
            solicitud.ClientIPAddress = _mKindo.fxObtenerIpEquipoActual(_parametrosSinpe.vHost).Result;
            solicitud.CultureCode = "ES-CR";
            solicitud.UserCode = _parametrosSinpe.vUserCGP;
            return solicitud;
        }

        private static (string Id, int Tipo) IdentificacionSinpe(string? cedula)
        {
            var limpia = cedula?.Trim() ?? string.Empty;
            var tipo = Convert.ToInt32(MKindoServiceDb.Inferir(limpia).Codigo);
            return (MKindoServiceDb.MaskSinpeId(tipo, limpia), tipo);
        }

        private static string ConstruirDescripcion(TesTransaccion solicitud)
        {
            var detalle = Truncar(string.Concat(solicitud.Detalle1, solicitud.Detalle2, solicitud.Detalle3, solicitud.Detalle4, solicitud.Detalle5));
            if (detalle.Length < 15)
                detalle = Truncar(detalle.Replace("\r\n", "") + " Transferencia SINPE");

            return detalle;
        }

        private static string Truncar(string texto) => texto.Length > 255 ? texto[..255] : texto;
    }
}
