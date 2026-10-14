
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models.CxP;
using Galileo.Models.ERROR;
using Galileo.BusinessLogic.ProGrX.CxP;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCxPTiposProvController : ControllerBase
    {
        private readonly FrmCxPTiposProvBL _bl;

        public FrmCxPTiposProvController(IConfiguration config)
        {
            _bl = new FrmCxPTiposProvBL(config);
        }

        [HttpGet("ObtenerClasificacionProveedores")]
        public ErrorDto<List<TiposProveedorDto>> ObtenerClasificacionProveedores(int CodCliente)
        {
            return _bl.ObtenerClasificacionProveedores(CodCliente);
        }

        [HttpGet("ObtenerProveedores")]
        public ErrorDto<List<Proveedor>> ObtenerProveedores(int CodCliente)
        {
            return _bl.ObtenerProveedores(CodCliente);
        }

        [ValidateAntiForgeryToken]
        [HttpPost("TipoProveedor_Actualizar")]
        public ErrorDto TipoProveedor_Actualizar(TiposProveedorDto request)
        {
            return _bl.TipoProveedor_Actualizar(request);
        }

        [ValidateAntiForgeryToken]
        [HttpPost("TipoProveedor_Eliminar")]
        public ErrorDto TipoProveedor_Eliminar(TiposProveedorDto request)
        {
            return _bl.TipoProveedor_Eliminar(request);
        }

        [ValidateAntiForgeryToken]
        [HttpPost("TipoProveedor_Insertar")]
        public ErrorDto TipoProveedor_Insertar(TiposProveedorDto request)
        {
            return _bl.TipoProveedor_Insertar(request);
        }
    }
}
