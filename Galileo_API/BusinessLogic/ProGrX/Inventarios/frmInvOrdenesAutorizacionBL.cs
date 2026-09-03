using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using Newtonsoft.Json;

namespace Galileo.BusinessLogic
{
    public sealed class FrmInvOrdenesAutorizacionBl
    {
        private const int CodigoValidacion = -2;

        private const string MensajeFiltrosInvalidos =
            "Los filtros de consulta no tienen un formato v&aacute;lido.";

        private readonly FrmInvOrdenesAutorizacionDb _db;

        public FrmInvOrdenesAutorizacionBl(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            _db = new FrmInvOrdenesAutorizacionDb(config);
        }

        public ErrorDto<List<ResolucionTransaccionDto>>
            INV_OrdenesAutorizacion_Ordenes_Obtener(
                int CodEmpresa,
                string filtros)
        {
            if (string.IsNullOrWhiteSpace(filtros))
            {
                return DbHelper.CreateErrorResponse(
                    "Los filtros de consulta son requeridos.",
                    CodigoValidacion,
                    new List<ResolucionTransaccionDto>());
            }

            try
            {
                var filtrosConsulta =
                    JsonConvert.DeserializeObject<
                        InvOrdenesAutorizacionFiltros>(
                            filtros);

                return _db
                    .INV_OrdenesAutorizacion_Ordenes_Obtener(
                        CodEmpresa,
                        filtrosConsulta);
            }
            catch (JsonException)
            {
                return DbHelper.CreateErrorResponse(
                    MensajeFiltrosInvalidos,
                    CodigoValidacion,
                    new List<ResolucionTransaccionDto>());
            }
        }

        public ErrorDto
            INV_OrdenesAutorizacion_Ordenes_Autorizar(
                int CodEmpresa,
                InvOrdenesAutorizacionProcesarRequest request)
        {
            return _db
                .INV_OrdenesAutorizacion_Ordenes_Autorizar(
                    CodEmpresa,
                    request);
        }

        public ErrorDto
            INV_OrdenesAutorizacion_Ordenes_Rechazar(
                int CodEmpresa,
                InvOrdenesAutorizacionProcesarRequest request)
        {
            return _db
                .INV_OrdenesAutorizacion_Ordenes_Rechazar(
                    CodEmpresa,
                    request);
        }
    }
}