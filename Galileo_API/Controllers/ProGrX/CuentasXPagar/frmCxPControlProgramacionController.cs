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
    public class FrmCxPControlProgramacionController : ControllerBase
    {
        private readonly FrmCxPControlProgramacionBL _bl;
        public FrmCxPControlProgramacionController(IConfiguration config)
        {
            _bl = new FrmCxPControlProgramacionBL(config);
        }

        [HttpPost("PagosFactura_Obtener")]
        public ErrorDto<ProgramacionPagoLista> PagosFactura_Obtener(int CodEmpresa, int? pagina, int? paginacion, string? filtro, ConsultaPagosParam param)
        {
            return _bl.PagosFactura_Obtener(CodEmpresa, pagina, paginacion, filtro, param);
        }

        [HttpGet("CargosAdicionales_Obtener")]
        public ErrorDto<List<CargoAdicional>> CargosAdicionales_Obtener(int CodEmpresa)
        {
            return _bl.CargosAdicionales_Obtener(CodEmpresa);
        }

        [HttpGet("DetalleSaldos_Obtener")]
        public ErrorDto<SaldosInformacion> DetalleSaldos_Obtener(int CodEmpresa, string Cod_Factura, int Cod_Proveedor)
        {
            return _bl.DetalleSaldos_Obtener(CodEmpresa, Cod_Factura, Cod_Proveedor);
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

        [HttpGet("FacturaProgramacionEstado_Obtener")]
        public ErrorDto<FacturaProgramacionEstado> FacturaProgramacionEstado_Obtener(int CodEmpresa, string Cod_Factura, int Cod_Proveedor, string Tipo)
        {
            return _bl.FacturaProgramacionEstado_Obtener(CodEmpresa, Cod_Factura, Cod_Proveedor, Tipo);
        }

        [HttpGet("DetallePagos_Obtener")]
        public ErrorDto<List<DetallePago>> DetallePagos_Obtener(int CodEmpresa, string Cod_Factura, int Cod_Proveedor)
        {
            return _bl.DetallePagos_Obtener(CodEmpresa, Cod_Factura, Cod_Proveedor);
        }

        [HttpGet("TesoreriaDetalle_Obtener")]
        public ErrorDto<TesoreriaDetalle> TesoreriaDetalle_Obtener(int CodEmpresa, int Tesoreria)
        {
            return _bl.TesoreriaDetalle_Obtener(CodEmpresa, Tesoreria);
        }

        [HttpPost("SaldosProveedor_Actualizar")]
        public ErrorDto SaldosProveedor_Actualizar(int CodEmpresa, decimal Saldo, decimal Tipo_Cambio, int Cod_Proveedor)
        {
            return _bl.SaldosProveedor_Actualizar(CodEmpresa, Saldo, Tipo_Cambio, Cod_Proveedor);
        }

        [HttpPost("FacturaEstado_Actualizar")]
        public ErrorDto FacturaEstado_Actualizar(int CodEmpresa, string Cod_Factura, int Cod_Proveedor)
        {
            return _bl.FacturaEstado_Actualizar(CodEmpresa, Cod_Factura, Cod_Proveedor);
        }

        [HttpPost("CompraEstado_Actualizar")]
        public ErrorDto CompraEstado_Actualizar(int CodEmpresa, string Cod_Factura, int Cod_Proveedor)
        {
            return _bl.CompraEstado_Actualizar(CodEmpresa, Cod_Factura, Cod_Proveedor);
        }

        [HttpPost("Pago_Insertar")]
        public ErrorDto Pago_Insertar(int CodEmpresa, DetallePago data)
        {
            return _bl.Pago_Insertar(CodEmpresa, data);
        }

        [HttpPost("PagoProvCargo_Insertar")]
        public ErrorDto PagoProvCargo_Insertar(int CodEmpresa, PagoProvCargo data)
        {
            return _bl.PagoProvCargo_Insertar(CodEmpresa, data);
        }

        [HttpGet("Disponible_Obtener")]
        public ErrorDto<Disponible> Disponible_Obtener(int CodEmpresa, int NPago, string Cod_Factura, int Cod_Proveedor)
        {
            return _bl.Disponible_Obtener(CodEmpresa, NPago, Cod_Factura, Cod_Proveedor);
        }

        [HttpPost("PagoProv_Actualizar")]
        public ErrorDto PagoProv_Actualizar(int CodEmpresa, string Usuario, string Cod_Factura, int Cod_Proveedor)
        {
            return _bl.PagoProv_Actualizar(CodEmpresa, Usuario, Cod_Factura, Cod_Proveedor);
        }

        [HttpPost("FechaVencimiento_Actualizar")]
        public ErrorDto FechaVencimiento_Actualizar(int CodEmpresa, DetallePago data)
        {
            return _bl.FechaVencimiento_Actualizar(CodEmpresa, data);
        }
    }
}
