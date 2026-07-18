using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.CxP;
using Galileo.Models.ERROR;
using Newtonsoft.Json;

namespace Galileo.BusinessLogic.ProGrX.CxP
{
    public class FrmCxPProveedoresBL
    {
        private readonly FrmCxPProveedoresDB _db;

        public FrmCxPProveedoresBL(IConfiguration config)
        {
            _db = new FrmCxPProveedoresDB(config);
        }

        public ErrorDto<TablasListaGenericaModel> Proveedores_Obtener(int CodEmpresa, string filtro, string parametros)
        {
            FiltrosLazyLoadData jfiltro = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(filtro) ?? new FiltrosLazyLoadData();
            CxPProveedorFiltros jparametros = JsonConvert.DeserializeObject<CxPProveedorFiltros>(parametros) ?? new CxPProveedorFiltros();
            return _db.Proveedores_Obtener(CodEmpresa, jfiltro, jparametros);
        }

        public ErrorDto<ProveedorDto> ProveedorDetalle_Obtener(int CodEmpresa, int Cod_Proveedor)
        {
            return _db.ProveedorDetalle_Obtener(CodEmpresa, Cod_Proveedor);
        }

        public ErrorDto<List<TipoProveedor>> TiposProveedor_Obtener(int CodEmpresa)
        {
            return _db.TiposProveedor_Obtener(CodEmpresa);
        }

        public ErrorDto<List<CuentaDesembolso>> CuentasDesembolso_Obtener(int CodEmpresa)
        {
            return _db.CuentasDesembolso_Obtener(CodEmpresa);
        }

        public ErrorDto<List<Cuenta>> Cuentas_Obtener(int CodEmpresa, string? Identificacion)
        {
            return _db.Cuentas_Obtener(CodEmpresa, Identificacion);
        }

        public ErrorDto Proveedor_Actualizar(int CodEmpresa, ProveedorDto request)
        {
            return _db.Proveedor_Actualizar(CodEmpresa, request);
        }

        public ErrorDto Proveedor_Insertar(int CodEmpresa, ProveedorDto request)
        {
            return _db.Proveedor_Insertar(CodEmpresa, request);
        }

        public ErrorDto Proveedor_Borrar(int CodEmpresa, int Cod_Proveedor)
        {
            return _db.Proveedor_Borrar(CodEmpresa, Cod_Proveedor);
        }

        public int ConsultaAscDesc(int CodEmpresa, int Cod_Proveedor, string tipo, ProveedorDataFiltros filtro)
        {
            return _db.ConsultaAscDesc(CodEmpresa, Cod_Proveedor, tipo, filtro);
        }

        public ErrorDto<List<Autorizacion>> Autorizaciones_Obtener(int CodEmpresa, int Cod_Proveedor)
        {
            return _db.Autorizaciones_Obtener(CodEmpresa, Cod_Proveedor);
        }

        public ErrorDto Autorizacion_Actualizar(int CodEmpresa, Autorizacion request)
        {
            return _db.Autorizacion_Actualizar(CodEmpresa, request);
        }

        public ErrorDto Autorizacion_Insertar(int CodEmpresa, Autorizacion request)
        {
            return _db.Autorizacion_Insertar(CodEmpresa, request);
        }

        public ErrorDto Autorizacion_Borrar(int CodEmpresa, Autorizacion request)
        {
            return _db.Autorizacion_Borrar(CodEmpresa, request);
        }

        public ErrorDto<List<TipoSuspension>> TipoSuspension_Obtener(int CodEmpresa)
        {
            return _db.TipoSuspension_Obtener(CodEmpresa);
        }

        public ErrorDto<SuspensionLista> Suspensiones_Obtener(int CodEmpresa, int Cod_Proveedor, int? pagina, int? paginacion, string? filtro)
        {
            return _db.Suspensiones_Obtener(CodEmpresa, Cod_Proveedor, pagina, paginacion, filtro);
        }

        public ErrorDto Suspencion_InsertUpdate(int CodEmpresa, Suspension request)
        {
            return _db.Suspencion_InsertUpdate(CodEmpresa, request);
        }

        public int ValidaCedJuridica(int CodEmpresa, int Cod_Proveedor, string cedula)
        {
            return _db.ValidaCedJuridica(CodEmpresa, Cod_Proveedor, cedula);
        }

        public ErrorDto<CuentaDivisa> ObtenerDivisaCuenta(int CodEmpresa, string Cuenta)
        {
            return _db.ObtenerDivisaCuenta(CodEmpresa, Cuenta);
        }

        public ErrorDto<ProveedorFusion> ProveedorFusion_ObtenerDetalle(int CodEmpresa, int Cod_Proveedor)
        {
            return _db.ProveedorFusion_ObtenerDetalle(CodEmpresa, Cod_Proveedor);
        }

        public ErrorDto<ProveedorFusionLista> ProveedorFusion_ObtenerLista(int CodEmpresa, int Cod_Proveedor, int? pagina, int? paginacion, string? filtro)
        {
            return _db.ProveedorFusion_ObtenerLista(CodEmpresa, Cod_Proveedor, pagina, paginacion, filtro);
        }

        public ErrorDto<List<ProveedorUsuariosListaDatos>> ProveedorUsuariosLista_Obtener(int CodEmpresa, int Cod_Proveedor)
        {
            return _db.ProveedorUsuariosLista_Obtener(CodEmpresa, Cod_Proveedor);
        }

        public ErrorDto<List<ProveedorEventosListaDatos>> ProveedorEventosLista_Obtener(int CodEmpresa, int Cod_Proveedor)
        {
            return _db.ProveedorEventosLista_Obtener(CodEmpresa, Cod_Proveedor);
        }

        public ErrorDto ProveedorEventos_Asigna(int CodEmpresa, int Cod_Proveedor,
            int Evento, bool Activa, string usuario)
        { 
            return _db.ProveedorEventos_Asigna(CodEmpresa, Cod_Proveedor, Evento, Activa, usuario);
        }

        public ErrorDto CxPProveedoresUsuario_Agregar(int CodEmpresa, ProveedorUsuariosListaDatos datos)
        {
            return _db.CxPProveedoresUsuario_Agregar(CodEmpresa, datos);
        }

        public ErrorDto ProveedorUsuario_RenovarClaveWeb(int CodEmpresa, int CodProveedor, string usuario, string? email, string usuarioSesion)
        {
            return _db.ProveedorUsuario_RenovarClaveWeb(CodEmpresa, CodProveedor, usuario, email, usuarioSesion);
        }


        public ErrorDto<List<BitacoraProveedorDto>> BitacoraProveedor_Obtener(int CodCliente, int cod_proveedor)
        {
            return _db.BitacoraProducto_Obtener(CodCliente, cod_proveedor);
        }

        public async Task<ErrorDto<List<ProveedorDto>>> Proveedor_NotificacionVencimiento(int CodEmpresa)
        {
            return await _db.Proveedor_NotificacionVencimiento(CodEmpresa);
        }

        public ErrorDto ProveedorEstado_Obtener(int CodEmpresa, int Cod_Proveedor)
        {
            return _db.ProveedorEstado_Obtener(CodEmpresa, Cod_Proveedor);
        }
    }//end class
}//end namespace
