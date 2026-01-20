using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCprAnulacionCompraController : ControllerBase
    {
        private readonly FrmCprAnulacionCompraBL _bl;

        public FrmCprAnulacionCompraController(IConfiguration config)
        {
            _bl = new FrmCprAnulacionCompraBL(config);
        }

        [HttpGet("Compras_Obtener")]
        public ErrorDto<List<CompraDto>> Compras_Obtener(int CodEmpresa, string filtro)
        {
            return _bl.Compras_Obtener(CodEmpresa, filtro);
        }

        [HttpGet("Compra_Datos_Obtener")]
        public ErrorDto<CompraAnulacionDatosDto> Compra_Datos_Obtener(int CodEmpresa, string Cod_Compra)
        {
            return _bl.Compra_Datos_Obtener(CodEmpresa, Cod_Compra);
        }


        [HttpGet("CompraDetalles_Obtener")]
        public ErrorDto<List<CompraDetalleDto>> CompraDetalles_Obtener(int CodEmpresa, string Cod_Factura)
        {
            return _bl.CompraDetalles_Obtener(CodEmpresa, Cod_Factura);
        }


        [HttpGet("Compra_Obtener")]
        public ErrorDto<CompraAnulacionDto> Compra_Obtener(int CodEmpresa, string codCompra)
        {
            return _bl.Compra_Obtener(CodEmpresa, codCompra);
        }

        [HttpPost("Compra_Anular")]
        public ErrorDto Compra_Anular(int CodEmpresa, CompraAnulacionDto compraDto)
        {
            return _bl.Compra_Anular(CodEmpresa, compraDto);
        }

        [HttpPost("Compra_Anulacion_Datos_Obtener")]
        public ErrorDto<CompraAnulacionDatosDto> Compra_Anulacion_Datos_Obtener(int CodEmpresa, CompraAnulacionDatosRequestDto compraAnulacionDatosRequestDto)
        {
            return _bl.Compra_Anulacion_Datos_Obtener(CodEmpresa, compraAnulacionDatosRequestDto);
        }
    }
}
