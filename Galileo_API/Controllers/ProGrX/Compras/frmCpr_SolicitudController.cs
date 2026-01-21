using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;
using Microsoft.AspNetCore.Authorization;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class FrmCprSolicitudController : ControllerBase
    {
        private readonly FrmCprSolicitudBL _bl;
        public FrmCprSolicitudController(IConfiguration config)
        {
            _bl = new FrmCprSolicitudBL(config);
        }

        [HttpGet("CprSolicitudLista_Obtener")]
        public ErrorDto<CprSolicitudLista> CprSolicitudLista_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.CprSolicitudLista_Obtener(CodEmpresa, filtros);
        }

        [HttpGet("CprSolicitud_Obtener")]
        public ErrorDto<CprSolicitudDto> CprSolicitud_Obtener(int CodEmpresa, int cpr_id, string usuario)
        {
            return _bl.CprSolicitud_Obtener(CodEmpresa, cpr_id, usuario);
        }

        [HttpGet("CprSolicitud_Scroll")]
        public ErrorDto<CprSolicitudDto> CprSolicitud_Scroll(int CodEmpresa, int scroll, string usuario, string? codigo)
        {
            return _bl.CprSolicitud_Scroll(CodEmpresa, scroll, usuario, codigo);
        }

        [HttpPost("CprSolicitud_Guardar")]
        public ErrorDto CprSolicitud_Guardar(int CodEmpresa, bool Edita, CprSolicitudDto solicitud)
        {
            return _bl.CprSolicitud_Guardar(CodEmpresa, Edita, solicitud);
        }

        [HttpDelete("CprSolicitud_Eliminar")]
        public ErrorDto CprSolicitud_Eliminar(int CodEmpresa, int cpr_id)
        {
            return _bl.CprSolicitud_Eliminar(CodEmpresa, cpr_id);
        }

        [HttpGet("CprSolicitudBs_Obtener")]
        public ErrorDto<List<CprSolicitudBsDto>> CprSolicitudBs_Obtener(int CodEmpresa, int? cpr_id, string? cod_unidad)
        {
            return _bl.CprSolicitudBs_Obtener(CodEmpresa, cpr_id, cod_unidad);
        }

        [HttpPost("CprSolicitudBs_Guardar")]
        public ErrorDto CprSolicitudBs_Guardar(int CodEmpresa, bool editaBs, CprSolicitudBsDto solicitud)
        {
            return _bl.CprSolicitudBs_Guardar(CodEmpresa, editaBs, solicitud);
        }

        [HttpDelete("CprSolicitudBs_Eliminar")]
        public ErrorDto CprSolicitudBs_Eliminar(int CodEmpresa, int cpr_id, string cod_producto, string cod_unidad)
        {
            return _bl.CprSolicitudBs_Eliminar(CodEmpresa, cpr_id, cod_producto, cod_unidad);
        }

        [HttpGet("CprValoracionesLista_Obtener")]
        public ErrorDto<List<CprValoracionLista>> CprValoracionesLista_Obtener(int CodEmpresa)
        {
            return _bl.CprValoracionesLista_Obtener(CodEmpresa);
        }

        [HttpGet("CprUens_Obtener")]
        public ErrorDto<List<CprUensLista>> CprUens_Obtener(int CodEmpresa, string usuario)
        {
            return _bl.CprUens_Obtener(CodEmpresa, usuario);
        }

        [HttpGet("CprSolicitudUens_Obtener")]
        public ErrorDto<List<CprValoracionLista>> CprSolicitudUens_Obtener(int CodEmpresa)
        {
            return _bl.CprSolicitudUens_Obtener(CodEmpresa);
        }

        [HttpGet("CprSolicitudBuscaProdPlan_Obtener")]
        public ErrorDto CprSolicitudBuscaProdPlan_Obtener(int CodEmpresa, string cod_producto)
        {
            return _bl.CprSolicitudBuscaProdPlan_Obtener(CodEmpresa, cod_producto);
        }

        [HttpGet("CprSolicitudBuscaProdCantPlan_Obtener")]
        public ErrorDto CprSolicitudBuscaProdCantPlan_Obtener(int CodEmpresa, string cod_producto, float cantidad)
        {
            return _bl.CprSolicitudBuscaProdCantPlan_Obtener(CodEmpresa, cod_producto, cantidad);
        }

        [HttpGet("Segumiento_Obtener")]
        public ErrorDto<List<CprSolicitudSeguimientoDto>> Segumiento_Obtener(int CodCliente, int cod_solicitud)
        {
            return _bl.Segumiento_Obtener(CodCliente, cod_solicitud);
        }


        [HttpGet("CprSolicitudCotizacionBs_Obtener")]
        public ErrorDto<CprSolicitudCotizacionPrvBsLista> CprSolicitudCotizacionBs_Obtener(int CodEmpresa, int? cpr_id, string? cod_unidad, string? cod_cotizacion)
        {
            return _bl.CprSolicitudCotizacionBs_Obtener(CodEmpresa, cpr_id, cod_unidad, cod_cotizacion);
        }

        [HttpPost("AutorizaSolicitud")]
        public ErrorDto AutorizaSolicitud(int CodEmpresa, int CPR_ID, string usuario)
        {
            return _bl.AutorizaSolicitud(CodEmpresa, CPR_ID, usuario);
        }


        [HttpPost("DeniegaSolicitud")]
        public ErrorDto DeniegaSolicitud(int CodEmpresa, int CPR_ID, string usuario, string detalle_seguimiento)
        {
            return _bl.DeniegaSolicitud(CodEmpresa, CPR_ID, usuario, detalle_seguimiento);
        }

        [HttpGet("ValidaUsuarioSolicitud")]
        public ErrorDto ValidaUsuarioSolicitud(int CodEmpresa, string usuario, string permiso, string? cod_unidad)
        {
            return _bl.ValidaUsuarioSolicitud(CodEmpresa, usuario, permiso, cod_unidad);
        }

        [HttpGet("Articulos_Obtener")]
        public ErrorDto<ArticuloDataLista> Articulos_Obtener(int CodEmpresa, int? pagina, int? paginacion, string? filtro , string? cod_unidad)
        {
            return _bl.Articulos_Obtener(CodEmpresa, pagina, paginacion, filtro, cod_unidad);
        }

        [HttpGet("CprSolicitud_TipoExcepcion")]
        public ErrorDto CprSolicitud_TipoExcepcion(int CodCliente)
        {
            return _bl.CprSolicitud_TipoExcepcion(CodCliente);
        }

        [HttpGet("CprSolicitud_UsuariosSolicitantes_Obtener")]
        public ErrorDto<List<string>> CprSolicitud_UsuariosSolicitantes_Obtener(int CodEmpresa)
        {
            return _bl.CprSolicitud_UsuariosSolicitantes_Obtener(CodEmpresa);
        }

        [HttpGet("CprSolicitud_UsuariosEncargados_Obtener")]
        public ErrorDto<List<string>> CprSolicitud_UsuariosEncargados_Obtener(int CodEmpresa)
        {
            return _bl.CprSolicitud_UsuariosEncargados_Obtener(CodEmpresa);
        }

        [HttpGet("Encargados_Obtener")]
        public ErrorDto<List<EncargadosDto>> Encargados_Obtener(int CodEmpresa, int cod_unidad)
        {
            return _bl.Encargados_Obtener(CodEmpresa, cod_unidad);
        }
    }
}