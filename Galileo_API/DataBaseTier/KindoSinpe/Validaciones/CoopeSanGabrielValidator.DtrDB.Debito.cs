using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.KindoSinpe;

namespace Galileo_API.DataBaseTier
{
    /// <summary>
    /// Débito en Tiempo Real (DTR) para cobros a otros bancos: la cuenta de origen es la interna
    /// del cliente (se acredita) y la cuenta destino es la externa (se debita).
    /// </summary>
    public partial class CoopeSanGabrielValidator
    {
        // Resumen del servicio a cobrar que exige el DTR (1 a 20 caracteres).
        private const string ServicioCobroDtr = "Cobro SINPE";

        public ErrorDto EmitirDebitoDtr(int CodEmpresa, int Nsolicitud, DateTime vfecha, string vUsuario, int doc_base, int contador) =>
            EjecutarEmisionPIN(CodEmpresa, Nsolicitud, vfecha, vUsuario, contador, null, EnviarDebitoDtr);

        private ErrorDto EnviarDebitoDtr(EnvioPin envio, TesTransaccion solicitud)
        {
            if (!_sinpeDTR.IsServiceAvailable(_parametrosSinpe.UrlCGP_DTR ?? string.Empty, ContextoPIN(new ReqBase())).ServiceAvailable)
                return RechazarServicioNoDisponible(envio);

            var respuesta = _sinpeDTR.SendDebit(_parametrosSinpe.UrlCGP_DTR ?? string.Empty, ConstruirSolicitudDtr(solicitud, envio.ChannelRef));
            var resultadoEnvio = ConvertirResultadoDtr(respuesta.DTRSendingResult);
            if (!respuesta.IsSuccessful || resultadoEnvio is null)
            {
                var error = respuesta.Errors is { Length: > 0 } ? respuesta.Errors[0] : null;
                return RegistrarErrorEnvio(envio, error?.Message, error?.Code ?? RechazoEnvioNoConfirmado);
            }

            // KINDO aceptó el débito: queda en proceso hasta tener un estado final.
            _mKindo.ActualizarEstadoMovTransito(envio.CodEmpresa, envio.ChannelRef, resultadoEnvio.SINPERefNumber ?? string.Empty, EstadoTransitoProcesado, 0, string.Empty);

            var resultado = ConsultarEstadoFinalDtr(envio, resultadoEnvio);
            return resultado.State == EstadoPINPagada
                ? RegistrarExito(envio, resultado)
                : RegistrarNoExitoso(envio, resultado);
        }

        /// <summary>
        /// Re-consulta con GetDebitResult mientras el DTR esté en un estado no final (sección 8.6).
        /// </summary>
        private PINSendingResult ConsultarEstadoFinalDtr(EnvioPin envio, PINSendingResult resultado)
        {
            for (var intento = 1; intento < MaxConsultasEstadoPIN && EsEstadoPendiente(resultado.State); intento++)
            {
                Thread.Sleep(EsperaConsultaEstadoMs);
                var consulta = _sinpeDTR.GetDebitResult(
                    _parametrosSinpe.UrlCGP_DTR ?? string.Empty,
                    ContextoPIN(new ReqDTRInfoChannelRef { ChannelRefNumber = envio.ChannelRef }));
                resultado = ConvertirResultadoDtr(consulta.DTRSendingResult) ?? resultado;
            }

            return resultado;
        }

        /// <summary>
        /// Solicitud DTR: el cliente de la Entidad (origen, se acredita) sale de los datos de origen de la solicitud;
        /// el cliente externo (destino, se debita) sale de los datos de destino.
        /// </summary>
        private ReqSendingDynamic ConstruirSolicitudDtr(TesTransaccion solicitud, string channelRef)
        {
            var interno = IdentificacionSinpe(solicitud.CedulaOrigen);
            var externo = IdentificacionSinpe(solicitud.Codigo);

            return ContextoPIN(new ReqSendingDynamic
            {
                CoreIntegrationPoint = 1,
                CostCenter = 1,
                Debit = new DTR
                {
                    ChannelRefNumber = channelRef,
                    Service = ServicioCobroDtr,
                    Amount = Math.Round(solicitud.Monto, 4),
                    CurrencyCode = solicitud.Divisa == "DOL" ? E_CodigoMonedaISO4217.Dolares : E_CodigoMonedaISO4217.Colones,
                    Description = ConstruirDescripcion(solicitud),
                    OriginCustomer = new OriginCustomer
                    {
                        Id = interno.Id,
                        Name = solicitud.NombreOrigen,
                        IBAN = solicitud.CuentaOrigen,
                        CreditIBAN = true
                    },
                    DestinationCustomer = new DestinationCustomer
                    {
                        Id = externo.Id,
                        Name = solicitud.Beneficiario,
                        IBAN = solicitud.Cuenta
                    }
                }
            });
        }

        /// <summary>
        /// Convierte el resultado de DTR (estado en texto) a la forma común de PINSendingResult.
        /// </summary>
        private static PINSendingResult? ConvertirResultadoDtr(DTRSendingResult? dtr)
        {
            if (dtr is null)
                return null;

            short? estado = null;
            if (short.TryParse(dtr.State, out var estadoNumerico))
                estado = estadoNumerico;

            return new PINSendingResult
            {
                ChannelRefNumber = dtr.ChannelRefNumber,
                CGPRefNumber = dtr.CGPRefNumber,
                SINPERefNumber = dtr.SINPERefNumber,
                CBTrxNumber = dtr.CBTrxNumber,
                State = estado,
                RejectCode = dtr.RejectCode,
                RejectDescription = dtr.RejectDescription
            };
        }
    }
}
