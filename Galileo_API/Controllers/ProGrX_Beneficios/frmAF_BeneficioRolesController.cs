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
    [Authorize]
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

        /// <summary>Lista de grupos de beneficios con paginación, filtro y ordenamiento.</summary>
        [HttpGet("BeneficioGrupoLista_Obtener")]
        public ErrorDto<BeneficioGrupoDataLista> BeneficioGrupoLista_Obtener(int CodCliente, string? filtros)
            => _bl.BeneficioGrupoLista_Obtener(CodCliente, filtros);

        /// <summary>Exporta la lista de grupos aplicando el filtro vigente, sin paginar.</summary>
        [HttpGet("BeneficioGrupo_Exportar")]
        public ErrorDto<List<BeneficioGrupoData>> BeneficioGrupo_Exportar(int CodCliente, string? filtros)
            => _bl.BeneficioGrupo_Exportar(CodCliente, filtros);

        /// <summary>Lista de usuarios y su pertenencia a un grupo.</summary>
        [HttpGet("BeneficioUsuariosLista_Obtener")]
        public ErrorDto<BeneficioUsuariosDataLista> BeneficioUsuariosLista_Obtener(int CodCliente, string cod_grupo, string? filtros)
            => _bl.BeneficioUsuariosLista_Obtener(CodCliente, cod_grupo, filtros);

        /// <summary>Lista completa de grupos de beneficios.</summary>
        [HttpGet("BeneficioGrupoData_Obtener")]
        public ErrorDto<List<BeneficioGrupoData>> BeneficioGrupoData_Obtener(int CodCliente)
            => _bl.BeneficioGrupoData_Obtener(CodCliente);

        /// <summary>Asocia un usuario a un grupo de beneficios.</summary>
        [HttpPost("GrupoUsuario_Insertar")]
        public ErrorDto GrupoUsuario_Insertar(int CodCliente, string usuario, string cod_grupo)
            => _bl.GrupoUsuario_Insertar(CodCliente, usuario, cod_grupo);

        /// <summary>Desasocia un usuario de un grupo de beneficios.</summary>
        [HttpDelete("GrupoUsuario_Eliminar")]
        public ErrorDto GrupoUsuario_Eliminar(int CodCliente, string usuario, string cod_grupo)
            => _bl.GrupoUsuario_Eliminar(CodCliente, usuario, cod_grupo);

        /// <summary>Guarda un grupo de beneficios (inserta o actualiza).</summary>
        [HttpPost("BeneficioGrupo_Guardar")]
        public ErrorDto BeneficioGrupo_Guardar(int CodCliente, [FromBody] BeneficioGrupoData grupo)
            => _bl.BeneficioGrupo_Guardar(CodCliente, grupo);
    }
}
