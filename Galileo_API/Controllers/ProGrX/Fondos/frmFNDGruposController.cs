using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Fondos;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.Controllers.ProGrX.Fondos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmFndGruposController : ControllerBase
    {
        private readonly FrmFndGruposBl _BL;

        public FrmFndGruposController(IConfiguration config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            _BL = new FrmFndGruposBl(config);
        }

        [Authorize]
        [HttpGet("Fnd_Grupos_Obtener")]
        public ErrorDto<List<FndGrupoDto>> Fnd_Grupos_Obtener(int CodEmpresa)
        {
            return _BL.Fnd_Grupos_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpPost("Fnd_Grupos_Guardar")]
        public ErrorDto<FndGrupoDto> Fnd_Grupos_Guardar(int CodEmpresa, FndGrupoDto grupo)
        {
            return _BL.Fnd_Grupos_Guardar(CodEmpresa, grupo);
        }

        [Authorize]
        [HttpDelete("Fnd_Grupos_Eliminar")]
        public ErrorDto Fnd_Grupos_Eliminar(int CodEmpresa, string CodGrupo)
        {
            return _BL.Fnd_Grupos_Eliminar(CodEmpresa, CodGrupo);
        }

        [Authorize]
        [HttpGet("Fnd_Grupos_ObtenerPlanes")]
        public ErrorDto<List<FndPlanGrupoDto>> Fnd_Grupos_ObtenerPlanes(int CodEmpresa, string CodGrupo)
        {
            return _BL.Fnd_Grupos_ObtenerPlanes(CodEmpresa, CodGrupo);
        }

        [Authorize]
        [HttpPost("Fnd_Grupos_ActualizarAsignacionPlan")]
        public ErrorDto Fnd_Grupos_ActualizarAsignacionPlan(int CodEmpresa, string CodGrupo, string CodPlan, int CodOperadora, bool Checked)
        {
            return _BL.Fnd_Grupos_ActualizarAsignacionPlan(CodEmpresa, CodGrupo, CodPlan, CodOperadora, Checked);
        }
    }
}