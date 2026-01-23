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
    public class FrmCprCargadorFacturasController : ControllerBase
    {
        private readonly FrmCprCargadorFacturasBL _bl;
        public FrmCprCargadorFacturasController(IConfiguration config)
        {
            _bl = new FrmCprCargadorFacturasBL(config);
        }

        [HttpGet("Cargador_Facturas_Obtener")]
        public ErrorDto<CprFacturasXmlLista> Cargador_Facturas_Obtener(int CodEmpresa, int proveedor, string filtros)
        {
            return _bl.Cargador_Facturas_Obtener(CodEmpresa, proveedor, filtros);
        }

        [HttpGet("Cargador_Factura_ObtenerPorId")]
        public ErrorDto<CprFacturasXmlDto> Cargador_Factura_ObtenerPorId(int CodEmpresa, int id)
        {
            return _bl.Cargador_Factura_ObtenerPorId(CodEmpresa, id);
        }

        [HttpPost("Cargador_Facturas_Guardar")]
        public ErrorDto Cargador_Facturas_Guardar(int CodEmpresa, CprFacturasXmlDto request)
        {
            return _bl.Cargador_Facturas_Guardar(CodEmpresa, request);
        }

        [HttpPost("Cargador_Facturas_Actualizar")]
        public ErrorDto Cargador_Facturas_Actualizar(int CodEmpresa, CprFacturasXmlDto request)
        {
            return _bl.Cargador_Facturas_Actualizar(CodEmpresa, request);
        }

        [HttpGet("Cargador_FacturasDetalle_Obtener")]
        public ErrorDto<List<CprFacturasLineasXmlData>> Cargador_FacturasDetalle_Obtener(int CodEmpresa, int id, string? cod_proveedor)
        {
            return _bl.Cargador_FacturasDetalle_Obtener(CodEmpresa, id, cod_proveedor);
        }

        [HttpGet("Cargador_FacturasActivas_Obtener")]
        public ErrorDto<CprFacturasXmlLista> Cargador_FacturasActivas_Obtener(int CodEmpresa, int proveedor, string filtros)
        {
            return _bl.Cargador_FacturasActivas_Obtener(CodEmpresa, proveedor, filtros);
        }
    }
}