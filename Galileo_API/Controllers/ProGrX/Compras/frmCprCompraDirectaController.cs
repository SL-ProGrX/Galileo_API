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
    public class FrmCprCompraDirectaController : ControllerBase
    {
        private readonly FrmCprCompraDirectaBL _bl;

        public FrmCprCompraDirectaController(IConfiguration config)
        {
            _bl = new FrmCprCompraDirectaBL(config);
        }

        [HttpGet("CompraDirecta_Obtener")]
        public ErrorDto<CompraDirectaData?> CompraDirecta_Obtener(int CodEmpresa, string CodCompra, string CodOrden, int CodProveedor)
        {
            return _bl.CompraDirecta_Obtener(CodEmpresa, CodCompra, CodOrden, CodProveedor);
        }

        [HttpGet("CompraDirectaDetalle_Obtener")]
        public ErrorDto<CompraDirectaListaData> CompraDirectaDetalle_Obtener(int CodEmpresa, string filtros, string? CodFactura, int? Codproveedor)
        {
            return _bl.CompraDirectaDetalle_Obtener(CodEmpresa, filtros, CodFactura, Codproveedor);
        }

        [HttpPost("CompraDirecta_Insertar")]
        public ErrorDto CompraDirecta_Insertar(int CodEmpresa, CompraDirectaInsert orden)
        {
            return _bl.CompraDirecta_Insertar(CodEmpresa, orden);
        }

        [HttpPut("CostoArticulos_Actualiza")]
        public ErrorDto CostoArticulos_Actualiza(int CodEmpresa, string Usuario, string CodCompra)
        {
            return _bl.CostoArticulos_Actualiza(CodEmpresa, Usuario, CodCompra);
        }

        [HttpGet("ComprasDirectas_Lista_Obtener")]
        public ErrorDto<List<CompraDirectaResumenData?>> ComprasDirectas_Lista_Obtener(int CodEmpresa)
        {
            return _bl.CprCompraDirecta_Lista_Obtener(CodEmpresa);
        }
    }
}
