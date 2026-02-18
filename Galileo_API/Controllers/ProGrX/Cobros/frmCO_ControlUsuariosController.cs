using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Cobros;
using Galileo_API.Models.ProGrX.Cobros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Cobros
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCOControlUsuariosController : ControllerBase
    {
        private readonly FrmCOControlUsuariosBL BL;

        public FrmCOControlUsuariosController(IConfiguration config)
        {
            BL = new FrmCOControlUsuariosBL(config);
        }

        [Authorize]
        [HttpGet("CO_Usuarios_Obtener")]
        public ErrorDto<CoControlUsuariosData> CO_Usuarios_Obtener(int CodEmpresa, string usuario)
        {
            return BL.CO_Usuarios_Obtener(CodEmpresa, usuario);
        }
        [Authorize]
        [HttpGet("CO_Usuarios_Scroll_Obtener")]
        public ErrorDto<CoControlUsuariosData> CO_Usuarios_Scroll_Obtener(int CodEmpresa,int scrollCode,string? usuarioActual = null)

        {
            return BL.CO_Usuarios_Scroll_Obtener(CodEmpresa, scrollCode, usuarioActual);
        }
        [Authorize]
        [HttpGet("CO_Usuarios_Existe_Obtener")]
        public ErrorDto CO_Usuarios_Existe_Obtener(int CodEmpresa, string usuario)
        {
            return BL.CO_Usuarios_Existe_Obtener(CodEmpresa, usuario);
        }

        [Authorize]
        [HttpGet("CO_Usuarios_F4_Obtener")]
        public ErrorDto<CoControlUsuariosListaResult<CoControlUsuariosF4Item>> CO_Usuarios_F4_Obtener(int CodEmpresa, string? filtro)
        {
            return BL.CO_Usuarios_F4_Obtener(CodEmpresa, filtro);
        }
        [Authorize]
        [HttpGet("CO_Bancos_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CO_Bancos_Dropdown_Obtener(int CodEmpresa, string usuario_sesion)
        {
            return BL.CO_Bancos_Dropdown_Obtener(CodEmpresa, usuario_sesion);
        }
        [Authorize]
        [HttpPost("CO_Usuarios_Guardar")]
        public ErrorDto CO_Usuarios_Guardar(int CodEmpresa, CoControlUsuariosGuardarRequest req)
        {
            return BL.CO_Usuarios_Guardar(CodEmpresa, req);
        }
        [Authorize]
        [HttpDelete("CO_Usuarios_Eliminar")]
        public ErrorDto CO_Usuarios_Eliminar(int CodEmpresa, string usuario, string usuario_sesion)
        {
            return BL.CO_Usuarios_Eliminar(CodEmpresa, usuario, usuario_sesion);
        }
        [Authorize]
        [HttpGet("CO_Usuarios_Cuentas_Lista_Obtener")]
        public ErrorDto<CoControlUsuariosListaResult<CoControlUsuariosCuentasData>> CO_Usuarios_Cuentas_Lista_Obtener(
            int CodEmpresa, string cedula, string? filtro)
        {
            return BL.CO_Usuarios_Cuentas_Lista_Obtener(CodEmpresa, cedula, filtro);
        }
        [Authorize]
        [HttpGet("CO_Usuarios_Cuentas_Lista_Export")]
        public ErrorDto<CoControlUsuariosListaResult<CoControlUsuariosCuentasData>> CO_Usuarios_Cuentas_Lista_Export(
            int CodEmpresa, string cedula, string? filtro)
        {
            return BL.CO_Usuarios_Cuentas_Lista_Export(CodEmpresa, cedula, filtro);
        }
        [Authorize]
        [HttpGet("CO_Usuarios_Grupos_Lista_Obtener")]
        public ErrorDto<CoControlUsuariosListaResult<CoControlUsuariosGrupoItem>> CO_Usuarios_Grupos_Lista_Obtener(
            int CodEmpresa, string usuario)
        {
            return BL.CO_Usuarios_Grupos_Lista_Obtener(CodEmpresa, usuario);
        }
        [Authorize]
        [HttpPost("CO_Usuarios_Grupos_Asignar")]
        public ErrorDto CO_Usuarios_Grupos_Asignar(int CodEmpresa, CoControlUsuariosAsignacionRequest req)
        {
            return BL.CO_Usuarios_Grupos_Asignar(CodEmpresa, req);
        }

        [Authorize]
        [HttpGet("CO_Usuarios_Carteras_Lista_Obtener")]
        public ErrorDto<CoControlUsuariosListaResult<CoControlUsuariosCarteraItem>> CO_Usuarios_Carteras_Lista_Obtener(
            int CodEmpresa, string usuario)
        {
            return BL.CO_Usuarios_Carteras_Lista_Obtener(CodEmpresa, usuario);
        }
        [Authorize]
        [HttpPost("CO_Usuarios_Carteras_Asignar")]
        public ErrorDto CO_Usuarios_Carteras_Asignar(int CodEmpresa, CoControlUsuariosAsignacionRequest req)
        {
            return BL.CO_Usuarios_Carteras_Asignar(CodEmpresa, req);
        }
    }
}
