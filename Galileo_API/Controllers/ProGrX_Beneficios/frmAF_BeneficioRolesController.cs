using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Beneficios;

namespace Galileo_API.Controllers.ProGrX_Beneficios
{
    /// <summary>
    /// Endpoints de Roles/Grupos de Beneficios (frmAF_BeneficioRoles).
    /// </summary>
    [Route("api/frmAF_BeneficioRoles")]
    [ApiController]
    public class FrmAfBeneficioRolesController : ControllerBase
    {
        private readonly FrmAfBeneficioRolesBL _bl;

        public FrmAfBeneficioRolesController(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _bl = new FrmAfBeneficioRolesBL(config);
        }

        /// <summary>Lista de grupos de beneficios.</summary>
        [Authorize]
        [HttpGet("BeneficioGrupoLista_Obtener")]
        public ErrorDto<BeneficioGrupoDataLista> BeneficioGrupoLista_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
            => _bl.BeneficioGrupoLista_Obtener(CodCliente, pagina, paginacion, filtro);

        /// <summary>Lista de usuarios y su pertenencia a un grupo.</summary>
        [Authorize]
        [HttpGet("BeneficioUsuariosLista_Obtener")]
        public ErrorDto<BeneficioUsuariosDataLista> BeneficioUsuariosLista_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro, string cod_grupo)
            => _bl.BeneficioUsuariosLista_Obtener(CodCliente, pagina, paginacion, filtro, cod_grupo);

        /// <summary>Lista completa de grupos de beneficios.</summary>
        [Authorize]
        [HttpGet("BeneficioGrupoData_Obtener")]
        public ErrorDto<List<BeneficioGrupoData>> BeneficioGrupoData_Obtener(int CodCliente)
            => _bl.BeneficioGrupoData_Obtener(CodCliente);

        /// <summary>Asocia un usuario a un grupo de beneficios.</summary>
        [Authorize]
        [HttpPost("GrupoUsuario_Insertar")]
        public ErrorDto GrupoUsuario_Insertar(int CodCliente, string usuario, string cod_grupo)
            => _bl.GrupoUsuario_Insertar(CodCliente, usuario, cod_grupo);

        /// <summary>Desasocia un usuario de un grupo de beneficios.</summary>
        [Authorize]
        [HttpDelete("GrupoUsuario_Eliminar")]
        public ErrorDto GrupoUsuario_Eliminar(int CodCliente, string usuario, string cod_grupo)
            => _bl.GrupoUsuario_Eliminar(CodCliente, usuario, cod_grupo);

        /// <summary>Guarda un grupo de beneficios (inserta o actualiza).</summary>
        [Authorize]
        [HttpPost("BeneficioGrupo_Guardar")]
        public ErrorDto BeneficioGrupo_Guardar(int CodCliente, [FromBody] BeneficioGrupoData grupo)
            => _bl.BeneficioGrupo_Guardar(CodCliente, grupo);
    }
}
