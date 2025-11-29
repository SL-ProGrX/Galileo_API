using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.PRES;

namespace Galileo.Controllers
{
    [Route("api/frmPres_Usuarios")]
    [Route("api/FrmPresUsuarios")]
    [ApiController]
    public class FrmPresUsuariosController : ControllerBase
    {
        readonly FrmPresUsuariosBl _bl;
        public FrmPresUsuariosController(IConfiguration config)
        {
            _bl = new FrmPresUsuariosBl(config);
        }

        [Authorize]
        [HttpGet("Pres_Usuarios_Obtener")]
        public ErrorDto<List<PresUsuariosData>> Pres_Usuarios_Obtener(int CodEmpresa, string? Usuario)
        {
            return _bl.Pres_Usuarios_Obtener(CodEmpresa, Usuario);
        }

        [Authorize]
        [HttpGet("Pres_Contabilidades_Obtener")]
        public ErrorDto<List<PresContabilidadesData>> Pres_Contabilidades_Obtener(int CodEmpresa, string Usuario)
        {
            return _bl.Pres_Contabilidades_Obtener(CodEmpresa, Usuario);
        }

        [Authorize]
        [HttpGet("Pres_Unidades_Obtener")]
        public ErrorDto<List<PresUnidadesData>> Pres_Unidades_Obtener(int CodEmpresa, string Usuario, int CodContab)
        {
            return _bl.Pres_Unidades_Obtener(CodEmpresa, Usuario, CodContab);
        }

        [Authorize]
        [HttpPost("Pres_Usuarios_Registro_SP")]
        public ErrorDto Pres_Usuarios_Registro_SP(int CodEmpresa, PresUsuariosInsert request)
        {
            return _bl.Pres_Usuarios_Registro_SP(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("Pres_Unidades_Registro_SP")]
        public ErrorDto Pres_Unidades_Registro_SP(int CodEmpresa, PresUnidadesInsert request)
        {
            return _bl.Pres_Unidades_Registro_SP(CodEmpresa, request);
        }
    }
}