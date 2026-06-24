using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using Microsoft.AspNetCore.Authorization;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class FrmInvCambioPreciosController : ControllerBase
    {
        private readonly FrmInvCambioPreciosBL _bl;
        public FrmInvCambioPreciosController(IConfiguration config)
        {
            _bl = new FrmInvCambioPreciosBL(config);
        }

        [HttpGet("OrdenesDetalle_Obtener")]
        public ErrorDto<List<FacturaPrecioDetalleDto>> OrdenesDetalle_Obtener(int CodEmpresa, string CodFactura, int? CodProveedor)
        {
            return _bl.OrdenesDetalle_Obtener(CodEmpresa, CodFactura, CodProveedor);
        }

        [HttpPost("PreciosFactura_Actualiza")]
        public ErrorDto PreciosFactura_Actualiza(int CodEmpresa, FacturaPrecioDetalleDto request)
        {
            return _bl.PreciosFactura_Actualiza(CodEmpresa, request);
        }

        [HttpPost("CambiosPrecios_Actualizar")]
        public ErrorDto CambiosPrecios_Actualizar(int CodEmpresa, PrecioExcelDto precio)
        {
            return _bl.CambiosPrecios_Actualizar(CodEmpresa, precio);
        }
    }
}