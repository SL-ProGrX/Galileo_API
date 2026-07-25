using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Beneficios;

namespace Galileo_API.Controllers.ProGrX_Beneficios
{
    /// <summary>
    /// Endpoints del Pago de Productos de Beneficios (frmAF_BeneProdPago).
    /// </summary>
    [Route("api/frmAF_BeneProdPago")]
    [ApiController]
    public class FrmAfBeneProdPagoController : ControllerBase
    {
        private readonly FrmAfBeneProdPagoBL _bl;

        public FrmAfBeneProdPagoController(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _bl = new FrmAfBeneProdPagoBL(config);
        }

        /// <summary>Lista de productos asignados pendientes de entrega.</summary>
        [Authorize]
        [HttpGet("AfiBeneProdAsgLista_Obtener")]
        public ErrorDto<AfiBeneProdAsgDataList> AfiBeneProdAsgLista_Obtener(int CodCliente, string cod_beneficio, int? pagina, int? paginacion, string? filtro)
            => _bl.AfiBeneProdAsgLista_Obtener(CodCliente, cod_beneficio, pagina, paginacion, filtro);

        /// <summary>Beneficios con productos asignados pendientes de pago.</summary>
        [Authorize]
        [HttpGet("AfiBeneficios_Obtener")]
        public ErrorDto<List<AfiBeneProdData>> AfiBeneficios_Obtener(int CodCliente)
            => _bl.AfiBeneficios_Obtener(CodCliente);

        /// <summary>Detalle de productos asignados a un beneficio y consecutivo.</summary>
        [Authorize]
        [HttpGet("AfiBeneProdAsg_Obtener")]
        public ErrorDto<List<AfiBeneProdAsgData>> AfiBeneProdAsg_Obtener(int CodCliente, string consec, string cod_beneficio)
            => _bl.AfiBeneProdAsg_Obtener(CodCliente, consec, cod_beneficio);

        /// <summary>Procesa la entrega de productos de beneficios.</summary>
        [Authorize]
        [HttpPost("AfiBeneOtorga_Actualiza")]
        public ErrorDto AfiBeneOtorga_Actualiza(int CodCliente, [FromBody] string beneficio)
            => _bl.AfiBeneOtorga_Actualiza(CodCliente, beneficio);
    }
}
