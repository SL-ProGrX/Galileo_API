using Galileo.DataBaseTier.ProGrX_Beneficios;
using Galileo.Models;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic.ProGrX_Beneficios
{
    public class FrmAfBeneProdPagoBL
    {
        private const int CodigoValidacion = -2;
        private const string MensajeFiltrosInvalidos =
            "Los filtros enviados no son v&aacute;lidos.";

        private readonly FrmAfBeneProdPagoDB _db;

        public FrmAfBeneProdPagoBL(IConfiguration config)
        {
            _db = new FrmAfBeneProdPagoDB(
                config ??
                throw new ArgumentNullException(nameof(config)));
        }

        public ErrorDto<List<DropDownListaGenericaModel>>
            AF_BeneProdPago_Beneficios_Obtener(
                int CodEmpresa)
        {
            return _db.AF_BeneProdPago_Beneficios_Obtener(
                CodEmpresa);
        }

        public ErrorDto<AfiBeneProdAsgDataList>
            AF_BeneProdPago_Lista_Obtener(
                int CodEmpresa,
                string filtros)
        {
            AfiBeneProdPagoListaRequest request;

            try
            {
                request =
                    JsonConvert.DeserializeObject<
                        AfiBeneProdPagoListaRequest>(filtros)
                    ?? new AfiBeneProdPagoListaRequest();
            }
            catch (JsonException)
            {
                return new ErrorDto<AfiBeneProdAsgDataList>
                {
                    Code = CodigoValidacion,
                    Description = MensajeFiltrosInvalidos,
                    Result = new AfiBeneProdAsgDataList()
                };
            }

            return _db.AF_BeneProdPago_Lista_Obtener(
                CodEmpresa,
                request);
        }

        public ErrorDto<List<AfiBeneProdDetalleData>>
            AF_BeneProdPago_Detalle_Obtener(
                int CodEmpresa,
                int consec,
                string cod_beneficio)
        {
            return _db.AF_BeneProdPago_Detalle_Obtener(
                CodEmpresa,
                consec,
                cod_beneficio);
        }

        public ErrorDto AF_BeneProdPago_Entrega_Procesar(
            int CodEmpresa,
            AfiBeneProdPagoEntregaRequest request)
        {
            return _db.AF_BeneProdPago_Entrega_Procesar(
                CodEmpresa,
                request);
        }
    }
}