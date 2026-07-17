
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using Galileo.Models;
using Galileo.Models.CxP;
using Galileo.Models.ERROR;
using Galileo.BusinessLogic.ProGrX.CxP;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class FrmCxPProveedoresController : ControllerBase
    {
        private readonly FrmCxPProveedoresBL _bl;

        public FrmCxPProveedoresController(IConfiguration config)
        {
            _bl = new FrmCxPProveedoresBL(config);
        }

        [HttpGet("Proveedores_Obtener")]
        public ErrorDto<TablasListaGenericaModel> Proveedores_Obtener(int CodEmpresa, string filtro, string parametros)
        {
            return _bl.Proveedores_Obtener(CodEmpresa, filtro, parametros);
        }

        [HttpGet("ProveedorDetalle_Obtener")]
        public ErrorDto<ProveedorDto> ProveedorDetalle_Obtener(int CodEmpresa, int Cod_Proveedor)
        {
            return _bl.ProveedorDetalle_Obtener(CodEmpresa, Cod_Proveedor);
        }

        [HttpGet("TiposProveedor_Obtener")]
        public ErrorDto<List<TipoProveedor>> TiposProveedor_Obtener(int CodEmpresa)
        {
            return _bl.TiposProveedor_Obtener(CodEmpresa);
        }

        [HttpGet("CuentasDesembolso_Obtener")]
        public ErrorDto<List<CuentaDesembolso>> CuentasDesembolso_Obtener(int CodEmpresa)
        {
            return _bl.CuentasDesembolso_Obtener(CodEmpresa);
        }

        [HttpGet("Cuentas_Obtener")]
        public ErrorDto<List<Cuenta>> Cuentas_Obtener(int CodEmpresa, string? Identificacion)
        {
            return _bl.Cuentas_Obtener(CodEmpresa, Identificacion);
        }

        [HttpPost("Proveedor_Actualizar")]
        public ErrorDto Proveedor_Actualizar(int CodEmpresa, ProveedorDto request)
        {
            return _bl.Proveedor_Actualizar(CodEmpresa, request);
        }

        [HttpPost("Proveedor_Insertar")]
        public ErrorDto Proveedor_Insertar(int CodEmpresa, ProveedorDto request)
        {
            return _bl.Proveedor_Insertar(CodEmpresa, request);
        }


        [HttpPost("Proveedor_Borrar")]
        public ErrorDto Proveedor_Borrar(int CodEmpresa, int Cod_Proveedor)
        {
            return _bl.Proveedor_Borrar(CodEmpresa, Cod_Proveedor);
        }

        [HttpGet("ConsultaAscDesc")]
        public int ConsultaAscDesc(int CodEmpresa, int Cod_Proveedor, string tipo, string filtro)
        {
            var jFiltro = JsonConvert.DeserializeObject<ProveedorDataFiltros>(filtro) ?? new ProveedorDataFiltros();
            return _bl.ConsultaAscDesc(CodEmpresa, Cod_Proveedor, tipo, jFiltro);
        }

        [HttpGet("Autorizaciones_Obtener")]
        public ErrorDto<List<Autorizacion>> Autorizaciones_Obtener(int CodEmpresa, int Cod_Proveedor)
        {
            return _bl.Autorizaciones_Obtener(CodEmpresa, Cod_Proveedor);
        }

        [HttpPost("Autorizacion_Actualizar")]
        public ErrorDto Autorizacion_Actualizar(int CodEmpresa, Autorizacion request)
        {
            return _bl.Autorizacion_Actualizar(CodEmpresa, request);
        }

        [HttpPost("Autorizacion_Insertar")]
        public ErrorDto Autorizacion_Insertar(int CodEmpresa, Autorizacion request)
        {
            return _bl.Autorizacion_Insertar(CodEmpresa, request);
        }

        [HttpPost("Autorizacion_Borrar")]
        public ErrorDto Autorizacion_Borrar(int CodEmpresa, Autorizacion request)
        {
            return _bl.Autorizacion_Borrar(CodEmpresa, request);
        }

        [HttpGet("TipoSuspension_Obtener")]
        public ErrorDto<List<TipoSuspension>> TipoSuspension_Obtener(int CodEmpresa)
        {
            return _bl.TipoSuspension_Obtener(CodEmpresa);
        }

        [HttpGet("Suspensiones_Obtener")]
        public ErrorDto<SuspensionLista> Suspensiones_Obtener(int CodEmpresa, int Cod_Proveedor, int? pagina, int? paginacion, string? filtro)
        {
            return _bl.Suspensiones_Obtener(CodEmpresa, Cod_Proveedor, pagina, paginacion, filtro);
        }

        [HttpPost("Suspencion_InsertUpdate")]
        public ErrorDto Suspencion_InsertUpdate(int CodEmpresa, Suspension request)
        {
            return _bl.Suspencion_InsertUpdate(CodEmpresa, request);
        }

        [HttpGet("ValidaCedJuridica")]
        public int ValidaCedJuridica(int CodEmpresa, int Cod_Proveedor, string Cedula)
        {
            return _bl.ValidaCedJuridica(CodEmpresa, Cod_Proveedor, Cedula);
        }

        [HttpGet("ObtenerDivisaCuenta")]
        public ErrorDto<CuentaDivisa> ObtenerDivisaCuenta(int CodEmpresa, string Cuenta)
        {
            return _bl.ObtenerDivisaCuenta(CodEmpresa, Cuenta);
        }

        [HttpGet("ProveedorFusion_ObtenerDetalle")]
        public ErrorDto<ProveedorFusion> ProveedorFusion_ObtenerDetalle(int CodEmpresa, int Cod_Proveedor)
        {
            return _bl.ProveedorFusion_ObtenerDetalle(CodEmpresa, Cod_Proveedor);
        }

        [HttpGet("ProveedorFusion_ObtenerLista")]
        public ErrorDto<ProveedorFusionLista> ProveedorFusion_ObtenerLista(int CodEmpresa, int Cod_Proveedor, int? pagina, int? paginacion, string? filtro)
        {
            return _bl.ProveedorFusion_ObtenerLista(CodEmpresa, Cod_Proveedor, pagina, paginacion, filtro);
        }

        [HttpGet("ProveedorUsuariosLista_Obtener")]
        public ErrorDto<List<ProveedorUsuariosListaDatos>> ProveedorUsuariosLista_Obtener(int CodEmpresa, int Cod_Proveedor)
        {
            return _bl.ProveedorUsuariosLista_Obtener(CodEmpresa, Cod_Proveedor);
        }

        [HttpGet("ProveedorEventosLista_Obtener")]
        public ErrorDto<List<ProveedorEventosListaDatos>> ProveedorEventosLista_Obtener(int CodEmpresa, int Cod_Proveedor)
        {
            return _bl.ProveedorEventosLista_Obtener(CodEmpresa, Cod_Proveedor);
        }

        [HttpPost("ProveedorEventos_Asigna")]
        public ErrorDto ProveedorEventos_Asigna(int CodEmpresa, int Cod_Proveedor,
            int Evento, bool Activa, string usuario)
        {
            return _bl.ProveedorEventos_Asigna(CodEmpresa, Cod_Proveedor, Evento, Activa, usuario);
        }

        [HttpPost("CxPProveedoresUsuario_Agregar")]
        public ErrorDto CxPProveedoresUsuario_Agregar(int CodEmpresa, ProveedorUsuariosListaDatos datos)
        {
            return _bl.CxPProveedoresUsuario_Agregar(CodEmpresa, datos);
        }

        [HttpPost("ProveedorUsuario_RenovarClaveWeb")]
        public ErrorDto ProveedorUsuario_RenovarClaveWeb(int CodEmpresa, int CodProveedor, string Usuario, string Email)
        {
            return _bl.ProveedorUsuario_RenovarClaveWeb(CodEmpresa, CodProveedor, Usuario, Email, User.Identity?.Name ?? string.Empty);
        }

        [HttpGet("BitacoraProveedor_Obtener")]
        public ErrorDto<List<BitacoraProveedorDto>> BitacoraProducto_Obtener(int CodCliente, int cod_proveedor)
        {
            return _bl.BitacoraProveedor_Obtener(CodCliente, cod_proveedor);
        }

        [HttpGet("Proveedor_NotificacionVencimiento")]
        public async Task<ErrorDto<List<ProveedorDto>>> Proveedor_NotificacionVencimiento(int CodEmpresa)
        {
            return await _bl.Proveedor_NotificacionVencimiento(CodEmpresa);
        }

        #region NO SÉ

        [HttpGet("ProveedorEstado_Obtener")]
        public ErrorDto ProveedorEstado_Obtener(int CodEmpresa, int Cod_Proveedor)
        {
            return _bl.ProveedorEstado_Obtener(CodEmpresa, Cod_Proveedor);
        }

        #endregion
    }
}
