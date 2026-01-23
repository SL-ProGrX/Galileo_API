using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;
using Microsoft.AspNetCore.Authorization;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class FrmCprOrdenesAutorizacionController : ControllerBase
    {
        private readonly FrmCprOrdenesAutorizacionBL _bl;

        public FrmCprOrdenesAutorizacionController(IConfiguration config)
        {
            _bl = new FrmCprOrdenesAutorizacionBL(config);
        }

        [HttpPost("OrdenCompra_Autorizacion_Obtener")]
        public ErrorDto<OrdenCompraDto> OrdenCompra_Autorizacion_Obtener(int CodEmpresa, int pagina, int paginacion, string? filtro, OrdenCompraRequestDto ordenCompraRequestDto)
        {
            return _bl.OrdenesCompra_Autorizacion_Obtener(CodEmpresa, pagina, paginacion, filtro, ordenCompraRequestDto);
        }

        [HttpPost("OrdenCompra_Autorizar")]
        public ErrorDto OrdenCompra_Autorizar(int CodEmpresa, OrdenCompraResolucionRequestDto ordenCompraResolucionRequestDto)
        {
            return _bl.OrdenCompra_Autorizar(CodEmpresa, ordenCompraResolucionRequestDto);
        }

        [HttpPost("OrdenCompra_Rechazar")]
        public ErrorDto OrdenCompra_Rechazar(int CodEmpresa, OrdenCompraResolucionRequestDto ordenCompraRequestDto)
        {
            return _bl.OrdenCompra_Rechazar(CodEmpresa, ordenCompraRequestDto);
        }
    }
}