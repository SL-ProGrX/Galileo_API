using Microsoft.AspNetCore.Mvc;
using Galileo.Models.CxP;
using Galileo.Models.ERROR;
using Microsoft.AspNetCore.Authorization;
using Galileo.BusinessLogic.ProGrX.CxP;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class FrmCxPControlReprogramacionController : ControllerBase
    {
        private readonly FrmCxPControlReprogramacionBL _bl;
        public FrmCxPControlReprogramacionController(IConfiguration config)
        {
            _bl = new FrmCxPControlReprogramacionBL(config);
        }

        [HttpGet("Facturas_Obtener")]
        public ErrorDto<FacturaLista> Facturas_Obtener(int CodEmpresa, int Cod_Proveedor, int? pagina, int? paginacion, string? filtro)
        {
            return _bl.Facturas_Obtener(CodEmpresa, Cod_Proveedor, pagina, paginacion, filtro);
        }

        [HttpGet("ProgramacionDetalle_Obtener")]
        public ErrorDto<VCxpProgramacionPago> ProgramacionDetalle_Obtener(int CodEmpresa, string Cod_Factura, int Cod_Proveedor)
        {
            return _bl.ProgramacionDetalle_Obtener(CodEmpresa, Cod_Factura, Cod_Proveedor);
        }

        [HttpGet("PagoMontos_Obtener")]
        public ErrorDto<Pago> PagoMontos_Obtener(int CodEmpresa, string Cod_Factura, int Cod_Proveedor)
        {
            return _bl.PagoMontos_Obtener(CodEmpresa, Cod_Factura, Cod_Proveedor);
        }

        [HttpPost("FacturaMonto_Ajuste")]
        public ErrorDto FacturaMonto_Ajuste(int CodEmpresa, AjusteFactura data)
        {
            return _bl.FacturaMonto_Ajuste(CodEmpresa, data);
        }

        [HttpGet("FacturaDetalle_Obtener")]
        public ErrorDto<FacturaDet> FacturaDetalle_Obtener(int CodEmpresa, string Cod_Factura, int Cod_Proveedor)
        {
            return _bl.FacturaDetalle_Obtener(CodEmpresa, Cod_Factura, Cod_Proveedor);
        }

        [HttpGet("CargosAdicionales_Obtener")]
        public ErrorDto<List<CargoAdicional>> CargosAdicionales_Obtener(int CodEmpresa, string Cod_Factura, int Cod_Proveedor)
        {
            return _bl.CargosAdicionales_Obtener(CodEmpresa, Cod_Factura, Cod_Proveedor);
        }

        [HttpGet("CompraDatos_Obtener")]
        public ErrorDto<FacturaDatos> CompraDatos_Obtener(int CodEmpresa, string Cod_Factura, int Cod_Proveedor)
        {
            return _bl.CompraDatos_Obtener(CodEmpresa, Cod_Factura, Cod_Proveedor);
        }

        [HttpGet("FacturaDatos_Obtener")]
        public ErrorDto<FacturaDatos> FacturaDatos_Obtener(int CodEmpresa, string Cod_Factura, int Cod_Proveedor)
        {
            return _bl.FacturaDatos_Obtener(CodEmpresa, Cod_Factura, Cod_Proveedor);
        }

        [HttpDelete("CargosPagos_Borrar")]
        public ErrorDto CargosPagos_Borrar(int CodEmpresa, int Pago, string Cod_Factura, int Cod_Proveedor)
        {
            return _bl.CargosPagos_Borrar(CodEmpresa, Pago, Cod_Factura, Cod_Proveedor);
        }

        [HttpPost("Reprogramacion_Aplicar")]
        public ErrorDto Reprogramacion_Aplicar(int CodEmpresa, ReprogramacionAplicar data)
        {
            return _bl.Reprogramacion_Aplicar(CodEmpresa, data);
        }
    }
}
