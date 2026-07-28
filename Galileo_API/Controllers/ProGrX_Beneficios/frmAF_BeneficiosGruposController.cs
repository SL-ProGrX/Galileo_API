using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Beneficios;

namespace Galileo_API.Controllers.ProGrX_Beneficios
{
    /// <summary>
    /// Endpoints de Grupos de Beneficios (frmAF_BeneficiosGrupos).
    /// </summary>
    [Route("api/frmAF_BeneficiosGrupos")]
    [ApiController]
    public class FrmAfBeneficiosGruposController : ControllerBase
    {
        private readonly FrmAfBeneficiosGruposBL _bl;

        public FrmAfBeneficiosGruposController(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _bl = new FrmAfBeneficiosGruposBL(config);
        }

        /// <summary>Lista de grupos de beneficios.</summary>
        [Authorize]
        [HttpGet("AfiBeneGrupos_Obtener")]
        public ErrorDto<AfiBeneGruposLista> AfiBeneGrupos_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
            => _bl.AfiBeneGrupos_Obtener(CodCliente, pagina, paginacion, filtro);

        /// <summary>Lista de beneficios y su marca de asignación a un grupo.</summary>
        [Authorize]
        [HttpGet("BeneficioUsuariosLista_Obtener")]
        public ErrorDto<AfiBeneGruposAsigandosLista> BeneficioUsuariosLista_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro, string cod_grupo)
            => _bl.BeneficioUsuariosLista_Obtener(CodCliente, pagina, paginacion, filtro, cod_grupo);

        /// <summary>Lista simple de grupos de beneficios.</summary>
        [Authorize]
        [HttpGet("AfiBeneGrupos_lista")]
        public ErrorDto<List<AfiBeneGrupos>> AfiBeneGrupos_lista(int CodCliente)
            => _bl.AfiBeneGrupos_lista(CodCliente);

        /// <summary>Catálogo de categorías de beneficios activas.</summary>
        [Authorize]
        [HttpGet("AfiBeneCategoriaLista_Obtener")]
        public ErrorDto<List<AfiBeneLista>> AfiBeneCategoriaLista_Obtener(int CodCliente)
            => _bl.AfiBeneCategoriaLista_Obtener(CodCliente);

        /// <summary>Exporta la lista completa de grupos.</summary>
        [Authorize]
        [HttpGet("AfiBeneGrupoExportar")]
        public ErrorDto<List<AfiBeneGrupos>> AfiBeneGrupoExportar(int CodCliente)
            => _bl.AfiBeneGrupoExportar(CodCliente);

        /// <summary>Obtiene las asignaciones de un grupo por tipo.</summary>
        [Authorize]
        [HttpGet("AfiAsignaciones_Obtener")]
        public ErrorDto<List<AfiBeneAsignacionesData>> AfiAsignaciones_Obtener(int CodCliente, int asigna, string grupo)
            => _bl.AfiAsignaciones_Obtener(CodCliente, asigna, grupo);

        /// <summary>Guarda un grupo de beneficios (inserta o actualiza).</summary>
        [Authorize]
        [HttpPost("AfiBeneGrupo_Guardar")]
        public ErrorDto AfiBeneGrupo_Guardar(int CodCliente, [FromBody] AfiBeneGrupos grupo)
            => _bl.AfiBeneGrupo_Guardar(CodCliente, grupo);

        /// <summary>Elimina un grupo de beneficios.</summary>
        [Authorize]
        [HttpDelete("AfiBeneGrupos_Eliminar")]
        public ErrorDto AfiBeneGrupos_Eliminar(int CodCliente, int cod_grupo)
            => _bl.AfiBeneGrupos_Eliminar(CodCliente, cod_grupo);

        /// <summary>Asocia un beneficio a un grupo.</summary>
        [Authorize]
        [HttpPost("AfiGrupoBeneficio_Insertar")]
        public ErrorDto AfiGrupoBeneficio_Insertar(int CodCliente, [FromBody] AfiGrupoBeneficioData grupo)
            => _bl.AfiGrupoBeneficio_Insertar(CodCliente, grupo);

        /// <summary>Desasocia un beneficio de un grupo.</summary>
        [Authorize]
        [HttpDelete("AfiGrupoBeneficio_Eliminar")]
        public ErrorDto AfiGrupoBeneficio_Eliminar(int CodCliente, [FromBody] AfiGrupoBeneficioData grupo)
            => _bl.AfiGrupoBeneficio_Eliminar(CodCliente, grupo);

        /// <summary>Registra una asignación de grupo por tipo.</summary>
        [Authorize]
        [HttpPost("AfiAsignaciones_Actualizar")]
        public ErrorDto AfiAsignaciones_Actualizar(int CodCliente, int asigna, string grupo, string valor, string usuario, string mov)
            => _bl.AfiAsignaciones_Actualizar(CodCliente, asigna, grupo, valor, usuario, mov);
    }
}
