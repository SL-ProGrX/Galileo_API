using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic
{
    public class FrmCprSolicitudBL
    {
        private readonly FrmCprSolicitudDB _db;

        public FrmCprSolicitudBL(IConfiguration config)
        {
            _db = new FrmCprSolicitudDB(config);
        }

        public ErrorDto<CprSolicitudLista> CprSolicitudLista_Obtener(int CodEmpresa, string filtros)
        {
            return _db.CprSolicitudLista_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto<CprSolicitudDto> CprSolicitud_Obtener(int CodEmpresa, int cpr_id, string usuario)
        {
            return _db.CprSolicitud_Obtener(CodEmpresa, cpr_id, usuario);
        }

        public ErrorDto<CprSolicitudDto> CprSolicitud_Scroll(int CodEmpresa, int scroll, string usuario, string? codigo)
        {
            return _db.CprSolicitud_Scroll(CodEmpresa, scroll, usuario, codigo);
        }

        public ErrorDto CprSolicitud_Guardar(int CodEmpresa, bool Edita, CprSolicitudDto solicitud)
        {
            return _db.CprSolicitud_Guardar(CodEmpresa, Edita, solicitud);
        }

        public ErrorDto CprSolicitud_Eliminar(int CodEmpresa, int cpr_id)
        {
            return _db.CprSolicitud_Eliminar(CodEmpresa, cpr_id);
        }

        public ErrorDto<List<CprSolicitudBsDto>> CprSolicitudBs_Obtener(int CodEmpresa, int? cpr_id, string? cod_unidad)
        {
            return _db.CprSolicitudBs_Obtener(CodEmpresa, cpr_id, cod_unidad);
        }

        public ErrorDto CprSolicitudBs_Guardar(int CodEmpresa, bool editaBs, CprSolicitudBsDto solicitud)
        {
            return _db.CprSolicitudBs_Guardar(CodEmpresa, editaBs, solicitud);
        }

        public ErrorDto CprSolicitudBs_Eliminar(int CodEmpresa, int cpr_id, string cod_producto, string cod_unidad)
        {
            return _db.CprSolicitudBs_Eliminar(CodEmpresa, cpr_id, cod_producto, cod_unidad);
        }

        public ErrorDto<List<CprValoracionLista>> CprValoracionesLista_Obtener(int CodEmpresa)
        {
            return _db.CprValoracionesLista_Obtener(CodEmpresa);
        }

        public ErrorDto<List<CprUensLista>> CprUens_Obtener(int CodEmpresa, string usuario)
        {
            return _db.CprUens_Obtener(CodEmpresa, usuario);
        }

        public ErrorDto<List<CprValoracionLista>> CprSolicitudUens_Obtener(int CodEmpresa)
        {
            return _db.CprSolicitudUens_Obtener(CodEmpresa);
        }

        public ErrorDto CprSolicitudBuscaProdPlan_Obtener(int CodEmpresa, string cod_producto)
        {
            return _db.CprSolicitudBuscaProdPlan_Obtener(CodEmpresa, cod_producto);
        }

        public ErrorDto CprSolicitudBuscaProdCantPlan_Obtener(int CodEmpresa, string cod_producto, float cantidad)
        {
            return _db.CprSolicitudBuscaProdCantPlan_Obtener(CodEmpresa, cod_producto, cantidad);
        }

        public ErrorDto<List<CprSolicitudSeguimientoDto>> Segumiento_Obtener(int CodCliente, int cod_solicitud)
        {
            return _db.Segumiento_Obtener(CodCliente, cod_solicitud);
        }

        public ErrorDto<CprSolicitudCotizacionPrvBsLista> CprSolicitudCotizacionBs_Obtener(int CodEmpresa, int? cpr_id, string? cod_unidad, string? cod_cotizacion)
        {
            return _db.CprSolicitudCotizacionBs_Obtener(CodEmpresa, cpr_id, cod_unidad, cod_cotizacion ?? string.Empty);
        }

        public ErrorDto AutorizaSolicitud(int CodEmpresa, int CPR_ID, string usuario)
        {
            return _db.AutorizaSolicitud(CodEmpresa, CPR_ID, usuario);
        }

        public ErrorDto DeniegaSolicitud(int CodEmpresa, int CPR_ID, string usuario, string detalle_seguimiento)
        {
            return _db.DeniegaSolicitud(CodEmpresa, CPR_ID, usuario, detalle_seguimiento);
        }

        public ErrorDto ValidaUsuarioSolicitud(int CodEmpresa, string usuario, string permiso, string? cod_unidad)
        {
            return _db.ValidaUsuarioSolicitud(CodEmpresa, usuario, permiso,cod_unidad);
        }

        public ErrorDto<ArticuloDataLista> Articulos_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro, string? cod_unidad)
        {
            return _db.Articulos_Obtener(CodCliente, pagina, paginacion, filtro, cod_unidad);
        }

        public ErrorDto CprSolicitud_TipoExcepcion(int CodCliente)
        {
            return _db.CprSolicitud_TipoExcepcion(CodCliente);
        }

        public ErrorDto<List<string>> CprSolicitud_UsuariosSolicitantes_Obtener(int CodEmpresa)
        {
            return _db.CprSolicitud_UsuariosSolicitantes_Obtener(CodEmpresa);
        }

        public ErrorDto<List<string>> CprSolicitud_UsuariosEncargados_Obtener(int CodEmpresa)
        {
            return _db.CprSolicitud_UsuariosEncargados_Obtener(CodEmpresa);
        }
        public ErrorDto<List<EncargadosDto>> Encargados_Obtener(int CodEmpresa, int cod_unidad)
        {
            return _db.Encargados_Obtener(CodEmpresa, cod_unidad);
        }
    }
}