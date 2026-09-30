using Galileo.Models;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Beneficios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Beneficios
{
    [Route("api/frmAF_BeneProdPago")]
    [ApiController]
    [Authorize]
    public class FrmAfBeneProdPagoController : ControllerBase
    {
        private readonly FrmAfBeneProdPagoBL _bl;

        public FrmAfBeneProdPagoController(
            IConfiguration config)
        {
            _bl = new FrmAfBeneProdPagoBL(
                config ??
                throw new ArgumentNullException(nameof(config)));
        }

        [HttpGet("AF_BeneProdPago_Beneficios_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>>
            AF_BeneProdPago_Beneficios_Obtener(
                int CodEmpresa)
        {
            return _bl.AF_BeneProdPago_Beneficios_Obtener(
                CodEmpresa);
        }

        [HttpGet("AF_BeneProdPago_Lista_Obtener")]
        public ErrorDto<AfiBeneProdAsgDataList>
            AF_BeneProdPago_Lista_Obtener(
                int CodEmpresa,
                string filtros)
        {
            return _bl.AF_BeneProdPago_Lista_Obtener(
                CodEmpresa,
                filtros);
        }

        [HttpGet("AF_BeneProdPago_Detalle_Obtener")]
        public ErrorDto<List<AfiBeneProdDetalleData>>
            AF_BeneProdPago_Detalle_Obtener(
                int CodEmpresa,
                int consec,
                string cod_beneficio)
        {
            return _bl.AF_BeneProdPago_Detalle_Obtener(
                CodEmpresa,
                consec,
                cod_beneficio);
        }

        [HttpPut("AF_BeneProdPago_Entrega_Procesar")]
        public ErrorDto AF_BeneProdPago_Entrega_Procesar(
            int CodEmpresa,
            AfiBeneProdPagoEntregaRequest request)
        {
            return _bl.AF_BeneProdPago_Entrega_Procesar(
                CodEmpresa,
                request);
        }
    }
}