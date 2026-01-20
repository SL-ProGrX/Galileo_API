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
    public class FrmCprOrdNivelAutoController : ControllerBase
    {
        private readonly FrmCprOrdNivelAutoBL _bl;

        public FrmCprOrdNivelAutoController(IConfiguration config)
        {
            _bl = new FrmCprOrdNivelAutoBL(config);
        }

        [HttpGet("UsuariosAutorizadores_Obtener")]
        public ErrorDto<UsuariosAuthorizaLista> UsuariosAutorizadores_Obtener(int CodEmpresa, string Filtros)
        {
            return _bl.UsuariosAutorizadores_Obtener(CodEmpresa, Filtros);
        }

        [HttpPost("OrdenAutousers_Insertar")]
        public ErrorDto OrdenAutousers_Insertar(int CodEmpresa, string usuario, string usuario_asignado)
        {
            return _bl.OrdenAutousers_Insertar(CodEmpresa, usuario, usuario_asignado);
        }

        [HttpDelete("OrdenAutousers_Eliminar")]
        public ErrorDto OrdenAutousers_Eliminar(int CodEmpresa, string usuario, string usuario_asignado)
        {
            return _bl.OrdenAutousers_Eliminar(CodEmpresa, usuario, usuario_asignado);
        }

        [HttpPost("OrdenAutorizadores_Insertar")]
        public ErrorDto OrdenAutorizadores_Insertar(int CodEmpresa, string usuario)
        {
            return _bl.OrdenAutorizadores_Insertar(CodEmpresa, usuario);
        }

        [HttpDelete("OrdenAutorizadores_Eliminar")]
        public ErrorDto OrdenAutorizadores_Eliminar(int CodEmpresa, string usuario)
        {
            return _bl.OrdenAutorizadores_Eliminar(CodEmpresa, usuario);
        }

        [HttpGet("FechaCamnbioAutorizadores_Obtener")]
        public ErrorDto<UsuariosAuthorizaLista> FechaCamnbioAutorizadores_Obtener(int CodEmpresa, string filtro)
        {
            return _bl.FechaCamnbioAutorizadores_Obtener(CodEmpresa, filtro);
        }

        [HttpGet("ListaAutorizador_Obtener")]
        public ErrorDto<List<UsuariosAutorizaData>> ListaAutorizador_Obtener(int CodEmpresa, string filtro)
        {
            return _bl.ListaAutorizador_Obtener(CodEmpresa, filtro);
        }

        [HttpGet("ListaAutousers_Obtener")]
        public ErrorDto<UsuariosAuthorizaLista> ListaAutousers_Obtener(int CodEmpresa, string usuario, string filtro)
        {
            return _bl.ListaAutousers_Obtener(CodEmpresa, usuario, filtro);
        }

        [HttpPost("FechaCambioAutorizadores_Insertar")]
        public ErrorDto FechaCambioAutorizadores_Insertar(int CodEmpresa, string usuario, string registro_usuario)
        {
            return _bl.FechaCambioAutorizadores_Insertar(CodEmpresa, usuario, registro_usuario);
        }

        [HttpDelete("FechaCambioAutorizadores_Eliminar")]
        public ErrorDto FechaCambioAutorizadores_Eliminar(int CodEmpresa, string usuario)
        {
            return _bl.FechaCambioAutorizadores_Eliminar(CodEmpresa, usuario);
        }

        [HttpGet("ObtenerListaRangos")]
        public ErrorDto<List<RangosDto>> ObtenerListaRangos(int CodEmpresa)
        {
            return _bl.ObtenerListaRangos(CodEmpresa);
        }

        [HttpGet("obtenerRangoUsuarios")]
        public ErrorDto<List<RangosUsuariosDto>> obtenerRangoUsuarios(int CodCliente, string cod_rango, string cod_uen,string? filtro)
        {
            return _bl.obtenerRangoUsuarios(CodCliente, cod_rango, cod_uen,filtro);
        }

        [HttpPost("registroRangosUsuarios")]
        public ErrorDto registroRangosUsuarios(int CodCliente, string cod_rango, RangosUsuariosDto request)
        {
            return _bl.registroRangosUsuarios(CodCliente, cod_rango, request);
        }

        [HttpPost("Rangos_Agregar")]
        public ErrorDto Rangos_Agregar(int CodEmpresa, RangosDto request)
        {
            return _bl.Rangos_Agregar(CodEmpresa, request);
        }

        [HttpPatch("Rangos_Actualizar")]
        public ErrorDto Rangos_Actualizar(int CodEmpresa, RangosDto request)
        {
            return _bl.Rangos_Actualizar(CodEmpresa, request);
        }

        [HttpDelete("Rangos_Eliminar")]
        public ErrorDto Rangos_Eliminar(int CodEmpresa, string id)
        {
            return _bl.Rangos_Eliminar(CodEmpresa, id);
        }
    }
}