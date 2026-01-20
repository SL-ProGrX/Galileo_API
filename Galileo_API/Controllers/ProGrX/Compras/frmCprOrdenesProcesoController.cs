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
    public class FrmCprOrdenesProcesoController : ControllerBase
    {
        private readonly FrmCprOrdenesProcesoBL _bl;

        public FrmCprOrdenesProcesoController(IConfiguration config)
        {
            _bl = new FrmCprOrdenesProcesoBL(config);
        }

        [HttpGet("ProveedorOrden_Obtener")]
        public ErrorDto<List<ProveedorOrdenesData>> ProveedorOrden_Obtener(int CodEmpresa, string CodOrden)
        {
            return _bl.ProveedorOrden_Obtener(CodEmpresa, CodOrden);
        }

        [HttpPost("Cpr_Orden_Proceso")]
        public ErrorDto Cpr_Orden_Proceso(int CodEmpresa, OrdenProceso orden)
        {
            return _bl.Cpr_Orden_Proceso(CodEmpresa, orden);
        }

        [HttpGet("OrdenProceso_ReemplazarPin")]
        public ErrorDto OrdenProceso_ReemplazarPin(int CodEmpresa, bool pinIngreso, string pin, string CodOrden)
        {
            return _bl.OrdenProceso_ReemplazarPin(CodEmpresa, pinIngreso, pin, CodOrden);
        }

        [HttpGet("Orden_Autoriza")]
        public ErrorDto Orden_Autoriza(int CodEmpresa, string CodOrden, string usuario, int index)
        {
            return _bl.Orden_Autoriza(CodEmpresa, CodOrden, usuario, index);
        }

        [HttpGet("Orden_Rechaza")]
        public ErrorDto Orden_Rechaza(int CodEmpresa, string CodOrden, string usuario, int index)
        {
            return _bl.Orden_Rechaza(CodEmpresa, CodOrden, usuario, index);
        }
        [HttpGet("Orden_Cerrar")]
        public ErrorDto Orden_Cerrar(int CodEmpresa, string CodOrden)
        {
            return _bl.Orden_Cerrar(CodEmpresa, CodOrden);
        }

        [HttpGet("ProveedorEstado_Obtener")]
        public ErrorDto ProveedorEstado_Obtener(int CodEmpresa, int CodProveedor)
        {
            return _bl.ProveedorEstado_Obtener(CodEmpresa, CodProveedor);
        }
    }
}