using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Credito;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCrGruposTrabajoController : ControllerBase
    {
        private readonly FrmCrGruposTrabajoBL _bl;

        public FrmCrGruposTrabajoController(IConfiguration config)
        {
            _bl = new FrmCrGruposTrabajoBL(config);
        }

        [HttpGet("CR_GruposTrabajo_Grupos_Obtener")]
        public ErrorDto<List<CrGrupoTrabajoGrupoData>> CR_GruposTrabajo_Grupos_Obtener(int codEmpresa)
            => _bl.CR_GruposTrabajo_Grupos_Obtener(codEmpresa);

        [HttpPost("CR_GruposTrabajo_Grupos_Guardar")]
        public ErrorDto CR_GruposTrabajo_Grupos_Guardar(int codEmpresa, CrGrupoTrabajoGrupoGuardarRequest request)
            => _bl.CR_GruposTrabajo_Grupos_Guardar(codEmpresa, request);

        [HttpGet("CR_GruposTrabajo_GruposCombo_Obtener")]
        public ErrorDto<List<CrGrupoTrabajoGrupoComboData>> CR_GruposTrabajo_GruposCombo_Obtener(int codEmpresa)
            => _bl.CR_GruposTrabajo_GruposCombo_Obtener(codEmpresa);

        [HttpGet("CR_GruposTrabajo_Miembros_Obtener")]
        public ErrorDto<List<CrGrupoTrabajoMiembroData>> CR_GruposTrabajo_Miembros_Obtener(int codEmpresa, string codGrupo)
            => _bl.CR_GruposTrabajo_Miembros_Obtener(codEmpresa, codGrupo);

        [HttpPost("CR_GruposTrabajo_Miembros_Marcar")]
        public ErrorDto CR_GruposTrabajo_Miembros_Marcar(int codEmpresa, CrGrupoTrabajoMiembroMarcarRequest request)
            => _bl.CR_GruposTrabajo_Miembros_Marcar(codEmpresa, request);

        [HttpGet("CR_GruposTrabajo_Etiquetas_Obtener")]
        public ErrorDto<List<CrGrupoTrabajoEtiquetaData>> CR_GruposTrabajo_Etiquetas_Obtener(int codEmpresa, string codGrupo)
            => _bl.CR_GruposTrabajo_Etiquetas_Obtener(codEmpresa, codGrupo);

        [HttpPost("CR_GruposTrabajo_Etiquetas_Marcar")]
        public ErrorDto CR_GruposTrabajo_Etiquetas_Marcar(int codEmpresa, CrGrupoTrabajoEtiquetaMarcarRequest request)
            => _bl.CR_GruposTrabajo_Etiquetas_Marcar(codEmpresa, request);

        [HttpGet("CR_GruposTrabajo_Comites_Obtener")]
        public ErrorDto<List<CrGrupoTrabajoComiteData>> CR_GruposTrabajo_Comites_Obtener(int codEmpresa, string codGrupo)
            => _bl.CR_GruposTrabajo_Comites_Obtener(codEmpresa, codGrupo);

        [HttpPost("CR_GruposTrabajo_Comites_Marcar")]
        public ErrorDto CR_GruposTrabajo_Comites_Marcar(int codEmpresa, CrGrupoTrabajoComiteMarcarRequest request)
            => _bl.CR_GruposTrabajo_Comites_Marcar(codEmpresa, request);

        [HttpDelete("CR_GruposTrabajo_Grupos_Eliminar")]
        public ErrorDto CR_GruposTrabajo_Grupos_Eliminar(int codEmpresa, string usuario, string codGrupo)
            => _bl.CR_GruposTrabajo_Grupos_Eliminar(codEmpresa, usuario, codGrupo);
    }
}
