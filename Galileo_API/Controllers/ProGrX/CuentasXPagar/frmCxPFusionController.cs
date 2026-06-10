using Microsoft.AspNetCore.Mvc;
using Galileo.Models.CxP;
using Galileo.Models.ERROR;
using Galileo.BusinessLogic.ProGrX.CxP;
using Microsoft.AspNetCore.Authorization;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class FrmCxPFusionController : ControllerBase
    {
        private readonly FrmCxPFusionBL _bl;

        public FrmCxPFusionController(IConfiguration config)
        {
            _bl = new FrmCxPFusionBL(config);
        }

        [HttpGet("Proveedores_Obtener")]
        public ErrorDto<CxpProveedoresDataLista> Proveedores_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            return _bl.Proveedores_Obtener(CodCliente, pagina, paginacion, filtro);
        }

        [HttpPost("Fusion_Aplicar")]
        public ErrorDto Fusion_Aplicar(int CodCliente, int proveedor, List<CxpProveedorData> proveedores)
        {
            return _bl.Fusion_Aplicar(CodCliente, proveedor, proveedores);
        }
    }
}