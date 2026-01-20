using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;
using BodegaDto = Galileo.Models.CPR.BodegaDto;
using Microsoft.AspNetCore.Authorization;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class FrmCprCompraDevController : ControllerBase
    {
        private readonly FrmCprCompraDevBL _bl;

        public FrmCprCompraDevController(IConfiguration config)
        {
            _bl = new FrmCprCompraDevBL(config);
        }
        
        [HttpGet("ObtenerListaFacturas")]
        public ErrorDto<List<FacturasDto>> ObtenerListaFacturas(int CodEmpresa, int CodProveedor)
        {
            return _bl.ObtenerListaFacturas(CodEmpresa, CodProveedor);
        }

        [HttpGet("ObtenerFactura")]
        public ErrorDto<FacturaDto?> ObtenerFactura(int CodEmpresa, string CodFactura, int CodProveedor)
        {
            return _bl.ObtenerFactura(CodEmpresa, CodFactura, CodProveedor);
        }

        [HttpGet("ObtenerFacturaDetalle")]
        public ErrorDto<List<FacturaDetalleDto>> ObtenerFacturaDetalle(int CodEmpresa, string CodFactura, int CodProveedor)
        {
            return _bl.ObtenerFacturaDetalle(CodEmpresa, CodFactura, CodProveedor);
        }

        [HttpGet("ObtenerBodegas")]
        public ErrorDto<List<BodegaDto>> ObtenerBodegas(int CodEmpresa)
        {
            return _bl.ObtenerBodegas(CodEmpresa);
        }

        [HttpGet("VerificaFactura")]
        public ErrorDto VerificaFactura(int CodEmpresa, string CodFactura, int CodProveedor)
        {
            return _bl.VerificaFactura(CodEmpresa, CodFactura, CodProveedor);
        }

        [HttpGet("Devolucion_Obtener")]
        public ErrorDto<DevolucionData?> Devolucion_Obtener(int CodEmpresa, string CodDevolucion)
        {
            return _bl.Devolucion_Obtener(CodEmpresa, CodDevolucion);
        }

        [HttpGet("DevolucionDetalle_Obtener")]
        public ErrorDto<List<FacturaDetalleDto>> ObtenerDevolucionDetalle(int CodEmpresa, string CodDevolucion)
        {
            return _bl.ObtenerDevolucionDetalle(CodEmpresa, CodDevolucion);
        }

        [HttpGet("OrdenCompraDev_Obtener")]
        public ErrorDto<FacturaDto?> ObtenerOrdenCompraDev(int CodEmpresa, string CodFactura, int CodProveedor)
        {
            return _bl.ObtenerOrdenCompraDev(CodEmpresa, CodFactura, CodProveedor);
        }

        [HttpPost("Devolucion_Guardar")]
        public ErrorDto Devolucion_Guardar(int CodEmpresa, DevolucionInsert orden)
        {
            return _bl.Devolucion_Guardar(CodEmpresa, orden);
        }
    }
}