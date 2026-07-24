using Microsoft.AspNetCore.Mvc;
using Galileo.Models.CxP;
using Galileo.Models.ERROR;
using Galileo.BusinessLogic.ProGrX.CxP;
using Microsoft.AspNetCore.Authorization;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class FrmCxPFacturasController : ControllerBase
    {
        private readonly FrmCxPFacturasBL _bl;

        public FrmCxPFacturasController(IConfiguration config)
        {
            _bl = new FrmCxPFacturasBL(config);
        }

        [HttpGet("ParamIVA_Obtener")]
        public ErrorDto<List<ParametrosIva>> ParamIVA_Obtener(int CodEmpresa)
        {
            return _bl.ParamIVA_Obtener(CodEmpresa);
        }

        [HttpGet("DivisaLocal_Obtener")]
        public ErrorDto<DivisaLocal> DivisaLocal_Obtener(int CodEmpresa)
        {
            return _bl.DivisaLocal_Obtener(CodEmpresa);
        }

        [HttpGet("Divisas_Obtener")]
        public ErrorDto<List<Divisa>> Divisas_Obtener(int CodEmpresa)
        {
            return _bl.Divisas_Obtener(CodEmpresa);
        }

        [HttpGet("Unidades_Obtener")]
        public ErrorDto<List<Unidad>> Unidades_Obtener(int CodEmpresa)
        {
            return _bl.Unidades_Obtener(CodEmpresa);
        }

        [HttpGet("CentrosCosto_Obtener")]
        public ErrorDto<List<CentroCosto>> CentrosCosto_Obtener(int CodEmpresa, string Cod_Unidad)
        {
            return _bl.CentrosCosto_Obtener(CodEmpresa, Cod_Unidad);
        }

        [HttpGet("Facturas_Obtener")]
        public ErrorDto<FacturaLista> Facturas_Obtener(int CodEmpresa, int Cod_Proveedor, int? pagina, int? paginacion, string? filtro)
        {
            return _bl.Facturas_Obtener(CodEmpresa, Cod_Proveedor, pagina, paginacion, filtro);
        }

        [HttpGet("FacturaDetalle_Obtener")]
        public ErrorDto<FacturaDto> FacturaDetalle_Obtener(int CodEmpresa, string Cod_Factura, int Cod_Proveedor)
        {
            return _bl.FacturaDetalle_Obtener(CodEmpresa, Cod_Factura, Cod_Proveedor);
        }

        [HttpGet("FacturaAsientos_Obtener")]
        public ErrorDto<List<AsientoFactura>> FacturaAsientos_Obtener(int CodEmpresa, string Cod_Factura, int Cod_Proveedor)
        {
            return _bl.FacturaAsientos_Obtener(CodEmpresa, Cod_Factura, Cod_Proveedor);
        }

        [HttpPost("FacturaNumero_Cambiar")]
        public ErrorDto FacturaNumero_Cambiar(int CodEmpresa, FacturaCambioNo request)
        {
            return _bl.FacturaNumero_Cambiar(CodEmpresa, request);
        }

        [HttpPost("FacturaImpuesto_Actualizar")]
        public ErrorDto FacturaImpuesto_Actualizar(int CodEmpresa, FacturaImpuesto data)
        {
            return _bl.FacturaImpuesto_Actualizar(CodEmpresa, data);
        }

        [HttpGet("ProveedorFactura_Obtener")]
        public ErrorDto<ProveedorFactura> ProveedorFactura_Obtener(int CodEmpresa, int Cod_Proveedor)
        {
            return _bl.ProveedorFactura_Obtener(CodEmpresa, Cod_Proveedor);
        }

        [HttpGet("ConsultaAscDesc")]
        public ErrorDto<FacturaAntSig> ConsultaAscDesc(int CodEmpresa, string Cod_Factura, string tipo, int Cod_Proveedor = 0)
        {
            return _bl.ConsultaAscDesc(CodEmpresa, Cod_Factura, tipo, Cod_Proveedor);
        }

        [HttpPost("Factura_Anular")]
        public ErrorDto Factura_Anular(int CodEmpresa, FacturaAnular data)
        {
            return _bl.Factura_Anular(CodEmpresa, data);
        }

        [HttpGet("Plantillas_Obtener")]
        public ErrorDto<FacturaPlantillaLista> Plantillas_Obtener(int CodEmpresa, int? pagina, int? paginacion, string? filtro)
        {
            return _bl.Plantillas_Obtener(CodEmpresa, pagina, paginacion, filtro);
        }

        [HttpGet("PlantillaAsientos_Obtener")]
        public ErrorDto<List<AsientoFactura>> PlantillaAsientos_Obtener(int CodEmpresa, int Cod_Plantilla, string fecha, decimal total)
        {
            return _bl.PlantillaAsientos_Obtener(CodEmpresa, Cod_Plantilla, fecha, total);
        }

        [HttpGet("PlantillaFactura_Obtener")]
        public ErrorDto<List<Factura>> PlantillaFactura_Obtener(int CodEmpresa)
        {
            return _bl.PlantillaFactura_Obtener(CodEmpresa);
        }

        [HttpGet("CuentaProveedor_Obtener")]
        public ErrorDto<CuentaProveedor> CuentaProveedor_Obtener(int CodEmpresa, int Cod_Proveedor)
        {
            return _bl.CuentaProveedor_Obtener(CodEmpresa, Cod_Proveedor);
        }

        [HttpGet("TipoCambio_Obtener")]
        public decimal TipoCambio_Obtener(int CodEmpresa, string cod_Divisa, string Fecha)
        {
            return _bl.TipoCambio_Obtener(CodEmpresa, cod_Divisa, Fecha);
        }


        [HttpDelete("FacturaAsientos_Borrar")]
        public ErrorDto FacturaAsientos_Borrar(int CodEmpresa, string Cod_Factura, int Cod_Proveedor)
        {
            return _bl.FacturaAsientos_Borrar(CodEmpresa, Cod_Factura, Cod_Proveedor);
        }

        [HttpPost("SaldoPagarProv_Actualizar")]
        public ErrorDto SaldoPagarProv_Actualizar(int CodEmpresa, decimal Saldo, decimal Saldo_Divisa, int Cod_Proveedor)
        {
            return _bl.SaldoPagarProv_Actualizar(CodEmpresa, Saldo, Saldo_Divisa, Cod_Proveedor);
        }

        [HttpPost("FacturaAsiento_Insertar")]
        public ErrorDto FacturaAsiento_Insertar(int CodEmpresa, AsientoFactura data)
        {
            return _bl.FacturaAsiento_Insertar(CodEmpresa, data);
        }

        [HttpPost("PagoContado_Insertar")]
        public ErrorDto PagoContado_Insertar(int CodEmpresa, PagoContado data)
        {
            return _bl.PagoContado_Insertar(CodEmpresa, data);
        }

        [HttpPost("Factura_Insertar")]
        public ErrorDto Factura_Insertar(int CodEmpresa, FacturaDto data)
        {
            return _bl.Factura_Insertar(CodEmpresa, data);
        }

        [HttpPost("FacturaCompleta_Insertar")]
        public ErrorDto FacturaCompleta_Insertar(int CodEmpresa, FacturaGuardarCompleta data)
        {
            return _bl.FacturaCompleta_Insertar(CodEmpresa, data);
        }
    }
}
