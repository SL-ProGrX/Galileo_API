using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Galileo.Models.CxP;
using Galileo.Models.ERROR;
using Galileo.BusinessLogic.ProGrX.CxP;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class FrmCxPControlEjecucionController : ControllerBase
    {
        private readonly FrmCxPControlEjecucionBL _bl;
        public FrmCxPControlEjecucionController(IConfiguration config)
        {
            _bl = new FrmCxPControlEjecucionBL(config);
        }

        [HttpPost("SincronizaTesoreriaCxPReportes")]
        public ErrorDto SincronizaTesoreriaCxPReportes(int CodEmpresa)
        {
            return _bl.SincronizaTesoreriaCxPReportes(CodEmpresa);
        }

        [HttpGet("Proveedores_Obtener")]
        public ErrorDto<ProveedoresPagosLista> Proveedores_Obtener(
            [FromQuery] ProveedoresPagosFiltro filtros)
        {
            return _bl.Proveedores_Obtener(filtros);
        }

        [HttpGet("DivisaFuncional_Obtener")]
        public ErrorDto<Divisa> DivisaFuncional_Obtener(int CodEmpresa)
        {
            return _bl.DivisaFuncional_Obtener(CodEmpresa);
        }

        [HttpGet("CargosAdicionales_Obtener")]
        public ErrorDto<List<Cargo>> CargosAdicionales_Obtener(int CodEmpresa)
        {
            return _bl.CargosAdicionales_Obtener(CodEmpresa);
        }

        [HttpGet("Divisas_Obtener")]
        public ErrorDto<List<Divisa>> Divisas_Obtener(int CodEmpresa, int CodContabilidad)
        {
            return _bl.Divisas_Obtener(CodEmpresa, CodContabilidad);
        }

        [HttpGet("Usuarios_Obtener")]
        public ErrorDto<List<UsuarioEjecucion>> Usuarios_Obtener(int CodEmpresa)
        {
            return _bl.Usuarios_Obtener(CodEmpresa);
        }

        [HttpPost("FacturasPendientePago_Obtener")]
        public ErrorDto<List<FacturaPendientePago>> FacturasPendientePago_Obtener(int CodEmpresa, FactPenReq request)
        {
            return _bl.FacturasPendientePago_Obtener(CodEmpresa, request);
        }

        [HttpGet("DetalleProveedor_Obtener")]
        public ErrorDto<Detalle> DetalleProveedor_Obtener(int CodEmpresa, int Cod_Proveedor, string Vence)
        {
            return _bl.DetalleProveedor_Obtener(CodEmpresa, Cod_Proveedor, Vence);
        }

        [HttpPost("RevisionPagos_Reactivar")]
        public ErrorDto RevisionPagos_Reactivar(int CodEmpresa, string user)
        {
            return _bl.RevisionPagos_Reactivar(CodEmpresa, user);
        }

        [HttpGet("Autorizaciones_Obtener")]
        public ErrorDto<List<Autorizado>> Autorizaciones_Obtener(int CodEmpresa, int Cod_Proveedor)
        {
            return _bl.Autorizaciones_Obtener(CodEmpresa, Cod_Proveedor);
        }

        [HttpGet("Fusion_Obtener")]
        public ErrorDto<Fusion> Fusion_Obtener(int CodEmpresa, int Cod_Proveedor)
        {
            return _bl.Fusion_Obtener(CodEmpresa, Cod_Proveedor);
        }

        [HttpGet("CuentasDesembolso_Obtener")]
        public ErrorDto<List<CuentaDesembolso>> CuentasDesembolso_Obtener(int CodEmpresa)
        {
            return _bl.CuentasDesembolso_Obtener(CodEmpresa);
        }

        [HttpGet("InfoCuenta_Obtener")]
        public ErrorDto<InfoCuenta> InfoCuenta_Obtener(int CodEmpresa, int Cod_Banco)
        {
            return _bl.InfoCuenta_Obtener(CodEmpresa, Cod_Banco);
        }

        [HttpGet("CuentasBancarias_Obtener")]
        public ErrorDto<List<CuentaBancaria>> CuentasBancarias_Obtener(int CodEmpresa, string Identificacion, int BancoId, int DivisaCheck)
        {
            return _bl.CuentasBancarias_Obtener(CodEmpresa, Identificacion, BancoId, DivisaCheck);
        }

        [HttpGet("CargoPorcentual_Obtener")]
        public ErrorDto<List<CargoPorcentual>> CargoPorcentual_Obtener(int CodEmpresa, int Cod_Proveedor, string Vence)
        {
            return _bl.CargoPorcentual_Obtener(CodEmpresa, Cod_Proveedor, Vence);
        }

        [HttpGet("ConsultaAscDesc")]
        public ErrorDto<ProveedorPagos> ConsultaAscDesc(int CodEmpresa, int Cod_Proveedor, int CodContabilidad, string Vence, string tipo)
        {
            return _bl.ConsultaAscDesc(CodEmpresa, Cod_Proveedor, CodContabilidad, Vence, tipo);
        }

        [HttpGet("Proveedor_Obtener")]
        public ErrorDto<ProveedorPagos> Proveedor_Obtener(int CodEmpresa, int Cod_Proveedor, int CodContabilidad)
        {
            return _bl.Proveedor_Obtener(CodEmpresa, Cod_Proveedor, CodContabilidad);
        }

        [HttpPost("Detalle_Insertar")]
        public ErrorDto Detalle_Insertar(int CodEmpresa, TesTransAsiento data)
        {
            return _bl.Detalle_Insertar(CodEmpresa, data);
        }

        [HttpGet("MontoAnticipos_Obtener")]
        public ErrorDto<Anticipo> MontoAnticipos_Obtener(int CodEmpresa, int Cod_Proveedor)
        {
            return _bl.MontoAnticipos_Obtener(CodEmpresa, Cod_Proveedor);
        }

        [HttpPost("Tesoreria_Insertar")]
        public ErrorDto Tesoreria_Insertar(int CodEmpresa, TesTransacciones data)
        {
            return _bl.Tesoreria_Insertar(CodEmpresa, data);
        }

        [HttpGet("Tesoreria_Obtener")]
        public ErrorDto<TesTransacciones> Tesoreria_Obtener(int CodEmpresa, int NSolicitud)
        {
            return _bl.Tesoreria_Obtener(CodEmpresa, NSolicitud);
        }

        [HttpPost("EjecucionPagosCargos_Registra")]
        public ErrorDto EjecucionPagosCargos_Registra(int CodEmpresa, FacturaPendientePago data)
        {
            return _bl.EjecucionPagosCargos_Registra(CodEmpresa, data);
        }

        [HttpPost("EjecucionPagos_CargosFlotantes_Aplicar")]
        public ErrorDto EjecucionPagos_CargosFlotantes_Aplicar(int CodEmpresa, FacturaPendientePago data)
        {
            return _bl.EjecucionPagos_CargosFlotantes_Aplicar(CodEmpresa, data);
        }

        [HttpPost("EjecucionPagos_SaldosCargoPorc_Actualizar")]
        public ErrorDto EjecucionPagos_SaldosCargoPorc_Actualizar(int CodEmpresa)
        {
            return _bl.EjecucionPagos_SaldosCargoPorc_Actualizar(CodEmpresa);
        }

        [HttpGet("DesembolsoNetos_Obtener")]
        public ErrorDto<DesembolsoNetos> DesembolsoNetos_Obtener(int CodEmpresa, int Cod_Proveedor)
        {
            return _bl.DesembolsoNetos_Obtener(CodEmpresa, Cod_Proveedor);
        }

        [HttpPost("Indicadores_Actualizar")]
        public ErrorDto Indicadores_Actualizar(int CodEmpresa, PagoProvUpdate data)
        {
            return _bl.Indicadores_Actualizar(CodEmpresa, data);
        }

        [HttpPost("CancelacionCargos_Actualizar")]
        public ErrorDto CancelacionCargos_Actualizar(int CodEmpresa, int Cod_Proveedor, string Usuario)
        {
            return _bl.CancelacionCargos_Actualizar(CodEmpresa, Cod_Proveedor, Usuario);
        }

        [HttpPost("EjecucionPagos_TesoreriaDetalle_Actualizar")]
        public ErrorDto EjecucionPagos_TesoreriaDetalle_Actualizar(int CodEmpresa)
        {
            return _bl.EjecucionPagos_TesoreriaDetalle_Actualizar(CodEmpresa);
        }

        [HttpGet("CargosPer_Obtener")]
        public ErrorDto<List<CargoPer>> CargosPer_Obtener(int CodEmpresa, int Cod_Proveedor)
        {
            return _bl.CargosPer_Obtener(CodEmpresa, Cod_Proveedor);
        }

        [HttpGet("ProveedorTesoreria_Obtener")]
        public ErrorDto<ProveedorInfoEjecucion> ProveedorTesoreria_Obtener(int CodEmpresa, int Cod_Proveedor, int cod_contabilidad)
        {
            return _bl.ProveedorTesoreria_Obtener(CodEmpresa, Cod_Proveedor, cod_contabilidad);
        }

        [HttpGet("Anticipos_Obtener")]
        public ErrorDto<List<Anticipo>> Anticipos_Obtener(int CodEmpresa, int Cod_Proveedor)
        {
            return _bl.Anticipos_Obtener(CodEmpresa, Cod_Proveedor);
        }

        [HttpPost("EjecucionPagos_Aplicar")]
        public ErrorDto<EjecucionPagosResultado> EjecucionPagos_Aplicar(int CodEmpresa, EjecucionPagosAplicar data)
        {
            return _bl.EjecucionPagos_Aplicar(CodEmpresa, data);
        }
    }
}
