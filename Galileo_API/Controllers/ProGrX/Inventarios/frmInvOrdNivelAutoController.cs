using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using Microsoft.AspNetCore.Authorization;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class FrmInvOrdNivelAutoController : ControllerBase
    {
        private readonly FrmInvOrdNivelAutoBL _bl;

        public FrmInvOrdNivelAutoController(IConfiguration config)
        {
            _bl = new FrmInvOrdNivelAutoBL(config);
        }

        #region Autorizadores

        [HttpGet("Autorizadores_Obtener")]
        public ErrorDto<AutorizadorDataLista> Autorizadores_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            return _bl.Autorizadores_Obtener(CodCliente, pagina, paginacion, filtro);
        }

        [HttpGet("Autorizador_ObtenerTodos")]
        public ErrorDto<List<AutorizadorDto>> Autorizador_ObtenerTodos(int CodEmpresa)
        {
            return _bl.Autorizador_ObtenerTodos(CodEmpresa);
        }

        [HttpGet("Autorizador_Obtener")]
        public ErrorDto<List<AutorizadorDto>> Autorizador_Obtener(int CodEmpresa)
        {
            return _bl.Autorizador_Obtener(CodEmpresa);
        }

        [HttpPost("Autorizador_Insertar")]
        public ErrorDto Autorizador_Insertar(int CodEmpresa, AutorizadorDto request)
        {
            return _bl.Autorizador_Insertar(CodEmpresa, request);
        }

        [HttpPost("Autorizador_Eliminar")]
        public ErrorDto Autorizador_Eliminar(int CodEmpresa, AutorizadorDto request)
        {
            return _bl.Autorizador_Eliminar(CodEmpresa, request);
        }

        #endregion

        #region Usuarios A Cargo

        [HttpGet("UsuariosACargoAut_Obtener")]
        // [Authorize]
        public ErrorDto<UsuariosACargoDataLista> UsuariosACargoAut_Obtener(int CodCliente, string usuario, int? pagina, int? paginacion, string? filtro)
        {
            return _bl.UsuariosACargoAut_Obtener(CodCliente, usuario, pagina, paginacion, filtro);
        }

        [HttpGet("UsuariosACargo_Obtener")]
        public List<UsuarioaCargoDto> UsuariosACargo_Obtener(int CodEmpresa, string usuario)
        {
            return _bl.UsuariosACargo_Obtener(CodEmpresa, usuario);
        }

        [HttpPost("UsuarioACargo_Actualizar")]
        public ErrorDto UsuarioACargo_Actualizar(int CodEmpresa, UsuarioaCargoDto request)
        {
            return _bl.UsuarioACargo_Actualizar(CodEmpresa, request);
        }

        #endregion

        #region Cambios de Fecha

        [HttpGet("UsuariosCambioFch_Obtener")]
        public UsuariosCambioFchDataLista UsuariosCambioFch_Obtener(int CodCliente, string tipo, int? pagina, int? paginacion, string? filtro)
        {
            return _bl.UsuariosCambioFch_Obtener(CodCliente, tipo, pagina, paginacion, filtro);
        }

        [HttpGet("UsuariosCambioFecha_Obtener")]
        public List<UsuarioaCambioFechaDto> UsuariosCambioFecha_Obtener(int CodEmpresa, string tipo)
        {
            return _bl.UsuariosCambioFecha_Obtener(CodEmpresa, tipo);
        }

        [HttpPost("CambioFechas_Insertar")]
        public ErrorDto CambioFechas_Insertar(int CodEmpresa, UsuarioaCambioFechaDto request)
        {
            return _bl.CambioFechas_Insertar(CodEmpresa, request);
        }

        [HttpPost("CambioFechas_Eliminar")]
        public ErrorDto CambioFechas_Eliminar(int CodEmpresa, UsuarioaCambioFechaDto request)
        {
            return _bl.CambioFechas_Eliminar(CodEmpresa, request);
        }

        #endregion
    }
}
