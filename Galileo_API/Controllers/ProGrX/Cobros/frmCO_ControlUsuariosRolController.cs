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
    public class FrmCOControlUsuariosRolController : ControllerBase
    {
        private readonly FrmCOControlUsuariosRolBL BL;

        public FrmCOControlUsuariosRolController(IConfiguration config)
        {
            BL = new FrmCOControlUsuariosRolBL(config);
        }

        [Authorize]
        [HttpGet("CO_UsuariosRol_Usuarios_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CO_UsuariosRol_Usuarios_Dropdown_Obtener(int CodEmpresa, string? filtro)
        {
            return BL.CO_UsuariosRol_Usuarios_Dropdown_Obtener(CodEmpresa, filtro);
        }

        [Authorize]
        [HttpGet("CO_UsuariosRol_Antiguedad_Lista_Obtener")]
        public ErrorDto<CoControlUsuariosListaResult<CoControlUsuariosRolAntiguedadItem>> CO_UsuariosRol_Antiguedad_Lista_Obtener(int CodEmpresa, string usuario)
        {
            return BL.CO_UsuariosRol_Antiguedad_Lista_Obtener(CodEmpresa, usuario);
        }

        [Authorize]
        [HttpGet("CO_UsuariosRol_Garantias_Lista_Obtener")]
        public ErrorDto<CoControlUsuariosListaResult<CoControlUsuariosRolGarantiaItem>> CO_UsuariosRol_Garantias_Lista_Obtener(int CodEmpresa, string usuario)
        {
            return BL.CO_UsuariosRol_Garantias_Lista_Obtener(CodEmpresa, usuario);
        }

        [Authorize]
        [HttpGet("CO_UsuariosRol_Oficinas_Lista_Obtener")]
        public ErrorDto<CoControlUsuariosListaResult<CoControlUsuariosRolOficinaItem>> CO_UsuariosRol_Oficinas_Lista_Obtener(int CodEmpresa, string usuario)
        {
            return BL.CO_UsuariosRol_Oficinas_Lista_Obtener(CodEmpresa, usuario);
        }

        [Authorize]
        [HttpGet("CO_UsuariosRol_Instituciones_Lista_Obtener")]
        public ErrorDto<CoControlUsuariosListaResult<CoControlUsuariosRolInstitucionItem>> CO_UsuariosRol_Instituciones_Lista_Obtener(int CodEmpresa, string usuario)
        {
            return BL.CO_UsuariosRol_Instituciones_Lista_Obtener(CodEmpresa, usuario);
        }

        [Authorize]
        [HttpPost("CO_UsuariosRol_Antiguedad_Asignar")]
        public ErrorDto CO_UsuariosRol_Antiguedad_Asignar(int CodEmpresa, CoControlUsuariosRolAsignarAntiguedadRequest req)
        {
            return BL.CO_UsuariosRol_Antiguedad_Asignar(CodEmpresa, req);
        }

        [Authorize]
        [HttpPost("CO_UsuariosRol_Garantia_Asignar")]
        public ErrorDto CO_UsuariosRol_Garantia_Asignar(int CodEmpresa, CoControlUsuariosRolAsignarGarantiaRequest req)
        {
            return BL.CO_UsuariosRol_Garantia_Asignar(CodEmpresa, req);
        }

        [Authorize]
        [HttpPost("CO_UsuariosRol_Oficina_Asignar")]
        public ErrorDto CO_UsuariosRol_Oficina_Asignar(int CodEmpresa, CoControlUsuariosRolAsignarOficinaRequest req)
        {
            return BL.CO_UsuariosRol_Oficina_Asignar(CodEmpresa, req);
        }

        [Authorize]
        [HttpPost("CO_UsuariosRol_Institucion_Asignar")]
        public ErrorDto CO_UsuariosRol_Institucion_Asignar(int CodEmpresa, CoControlUsuariosRolAsignarInstitucionRequest req)
        {
            return BL.CO_UsuariosRol_Institucion_Asignar(CodEmpresa, req);
        }

        [Authorize]
        [HttpPost("CO_UsuariosRol_Copia")]
        public ErrorDto CO_UsuariosRol_Copia(int CodEmpresa, CoControlUsuariosRolCopiaRequest req)
        {
            return BL.CO_UsuariosRol_Copia(CodEmpresa, req);
        }

        [Authorize]
        [HttpPost("CO_UsuariosRol_Limpia")]
        public ErrorDto CO_UsuariosRol_Limpia(int CodEmpresa, CoControlUsuariosRolLimpiaRequest req)
        {
            return BL.CO_UsuariosRol_Limpia(CodEmpresa, req);
        }
    }
}
