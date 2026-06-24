using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCprComprasOrdenController : ControllerBase
    {
        private readonly FrmCprComprasOrdenBL _bl;

        public FrmCprComprasOrdenController(IConfiguration config)
        {
            _bl = new FrmCprComprasOrdenBL(config);
        }

        [HttpGet("Orden_Obtener")]
        public ErrorDto<OrdenCompraSinFacturaData> Orden_Obtener(int CodEmpresa, string CodOrden)
        {
            return _bl.Orden_Obtener(CodEmpresa, CodOrden);
        }

        [HttpGet("OrdenFactura_Obtener")]
        public ErrorDto<OrdenCompraFacturaData> OrdenFactura_Obtener(int CodEmpresa, string CodOrden)
        {
            return _bl.OrdenFactura_Obtener(CodEmpresa, CodOrden);
        }

        [HttpGet("OrdenesDetalle_Obtener")]
        public ErrorDto<CompraOrdenLineasData> OrdenesDetalle_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.OrdenesDetalle_Obtener(CodEmpresa, filtros);
        }

        [HttpPost("ComprasOrden_Guardar")]
        public ErrorDto ComprasOrden_Guardar(int CodEmpresa, ComprasOrdenDatos orden)
        {
            return _bl.ComprasOrden_Guardar(CodEmpresa, orden);
        }

        [HttpPatch("ComprasOrden_Actualizar")]
        public ErrorDto ComprasOrden_Actualizar(int CodEmpresa, ComprasOrdenDatos orden)
        {
            return _bl.ComprasOrden_Guardar(CodEmpresa, orden);
        }

        [HttpGet("OrdenPin_Obtener")]
        public ErrorDto OrdenPin_Obtener(int CodEmpresa, string CodOrden)
        {
            return _bl.OrdenPin_Obtener(CodEmpresa, CodOrden);
        }

        [HttpGet("OrdenPin_Verifica")]
        public ErrorDto OrdenPin_Verifica(int CodEmpresa, string CodOrden, string OrdPin)
        {
            return _bl.OrdenPin_Verifica(CodEmpresa, CodOrden, OrdPin);
        }

        [HttpGet("OrdenConsecutivo_Obtener")]
        public ErrorDto OrdenConsecutivo_Obtener(int CodEmpresa)
        {
            return _bl.OrdenConsecutivo_Obtener(CodEmpresa);
        }

        [HttpGet("FacturasAutorizar_Obtener")]
        public ErrorDto<List<FacturasAutorizarDto>> FacturasAutorizar_Obtener(int CodEmpresa, string usuario, int proveedor)
        {
            return _bl.FacturasAutorizar_Obtener(CodEmpresa, usuario, proveedor);
        }

        [HttpPost("Factura_AutorizarRechazar")]
        public ErrorDto Factura_AutorizarRechazar(int CodEmpresa,string usuario, string cod, string cod_factura, string justificacion)
        {
            return _bl.Factura_AutorizarRechazar(CodEmpresa, usuario, cod, cod_factura, justificacion);
        }

        [HttpPost("ValidaAutorizacion")]
        public ErrorDto ValidaAutorizacion(int CodEmpresa, string usuario, string cod_orden)
        {
            return _bl.ValidaAutorizacion(CodEmpresa, usuario, cod_orden);
        }
    }
}