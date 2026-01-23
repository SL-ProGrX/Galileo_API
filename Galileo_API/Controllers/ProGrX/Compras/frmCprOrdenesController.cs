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
    public class FrmCprOrdenesController : ControllerBase
    {
        private readonly FrmCprOrdenesBL _bl;

        public FrmCprOrdenesController(IConfiguration config)
        {
            _bl = new FrmCprOrdenesBL(config);
        }

        [HttpGet("OrdenesDetalle_Obtener")]
        public ErrorDto<OrdenDto> OrdenesDetalle_Obtener(int CodEmpresa, string CodOrden, string usuario)
        {
            return _bl.OrdenSeleccionadaObtener(CodEmpresa, CodOrden, usuario);
        }

        [HttpGet("OrdenesLineas_Obtener")]
        public ErrorDto<OrdenLineasData> OrdenesLineas_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.OrdenLineasObtener(CodEmpresa, filtros);
        }

        [HttpGet("Orden_Scroll")]
        public ErrorDto<OrdenesData> Orden_scroll(int CodEmpresa, int scrollValue, string? cod_Orden)
        {
            return _bl.Orden_scroll(CodEmpresa, scrollValue, cod_Orden);
        }

        [HttpPost("Orden_Insertar")]
        public ErrorDto Orden_Insertar(int CodEmpresa, object Orden)
        {
            return _bl.Orden_Insertar(CodEmpresa, Orden);
        }

        [HttpPut("Orden_Actualiza")]
        public ErrorDto Orden_Actualiza(int CodEmpresa, OrdenDatosAcciones Orden)
        {
            return _bl.Orden_Actualiza(CodEmpresa, Orden);
        }

        [HttpGet("OrdenesUENs_Obtener")]
        public ErrorDto<List<OrdenesUensData>> OrdenesUENs_Obtener(int CodEmpresa, string CodOrden, string CodProducto)
        {
            return _bl.OrdenesUENs_Obtener(CodEmpresa, CodOrden, CodProducto);
        }

        [HttpPost("OrdenesUENs_Guardar")]
        public ErrorDto OrdenesUENs_Guardar(int CodEmpresa, List<OrdenesUensData> lista)
        {
            return _bl.OrdenesUENs_Guardar(CodEmpresa, lista);
        }

        [HttpDelete("OrdenesUENs_Eliminar")]
        public ErrorDto OrdenesUENs_Eliminar(int CodEmpresa, string cod_orden, string cod_producto, string cod_unidad)
        {
            return _bl.OrdenesUENs_Eliminar(CodEmpresa, cod_orden, cod_producto, cod_unidad);
        }

        [HttpGet("horarios_Obtener")]
        public ErrorDto<List<CprHorarioLista>> horarios_Obtener(int CodEmpresa, string usuario)
        {
            return _bl.horarios_Obtener(CodEmpresa, usuario);
        }

        [HttpGet("formapago_Obtener")]
        public ErrorDto<List<CprFormaPago>> formapago_Obtener(int CodEmpresa, string usuario)
        {
            return _bl.formapago_Obtener(CodEmpresa, usuario);
        }

        [HttpPost("CorreoNotificaOrdenCompra")]
        public ErrorDto CorreoNotificaOrdenCompra(int CodEmpresa, string cod_orden , string proveedor, string cod_proveedor)
        {
            return _bl.CorreoNotificaOrdenCompra(CodEmpresa, cod_orden, proveedor, cod_proveedor);
        }
    }
}
