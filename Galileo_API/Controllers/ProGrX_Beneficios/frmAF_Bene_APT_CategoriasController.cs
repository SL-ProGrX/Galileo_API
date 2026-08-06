using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Beneficios;

namespace Galileo_API.Controllers.ProGrX_Beneficios
{
    /// <summary>
    /// Endpoints del catálogo de Categorías Apremiantes de Beneficios (frmAF_Bene_APT_Categorias).
    /// </summary>
    [Route("api/frmAF_Bene_APT_Categorias")]
    [ApiController]
    [Authorize]
    public class FrmAfBeneAptCategoriasController : ControllerBase
    {
        private readonly FrmAfBeneAptCategoriasBL _bl;

        public FrmAfBeneAptCategoriasController(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _bl = new FrmAfBeneAptCategoriasBL(config);
        }

        /// <summary>Lista de categorías apremiantes con paginación, filtro y ordenamiento.</summary>
        [HttpGet("CategoriasApremiante_Obtener")]
        public ErrorDto<AptCategoriasDataLista> CategoriasApremiante_Obtener(int CodEmpresa, string? filtros)
            => _bl.CategoriasApremiante_Obtener(CodEmpresa, filtros);

        /// <summary>Exporta la lista de categorías aplicando el filtro vigente, sin paginar.</summary>
        [HttpGet("CategoriasApremiante_Exportar")]
        public ErrorDto<List<AptCategorias>> CategoriasApremiante_Exportar(int CodEmpresa, string? filtros)
            => _bl.CategoriasApremiante_Exportar(CodEmpresa, filtros);

        /// <summary>Inserta una categoría apremiante.</summary>
        [HttpPost("CategoriasApremiante_Agregar")]
        public ErrorDto CategoriasApremiante_Agregar(int CodEmpresa, [FromBody] AptCategorias request)
            => _bl.CategoriasApremiante_Agregar(CodEmpresa, request);

        /// <summary>Actualiza una categoría apremiante.</summary>
        [HttpPut("CategoriasApremiante_Actualizar")]
        public ErrorDto CategoriasApremiante_Actualizar(int CodEmpresa, [FromBody] AptCategorias request)
            => _bl.CategoriasApremiante_Actualizar(CodEmpresa, request);

        /// <summary>Elimina una categoría apremiante.</summary>
        [HttpDelete("CategoriasApremiante_Eliminar")]
        public ErrorDto CategoriasApremiante_Eliminar(int CodEmpresa, int id)
            => _bl.CategoriasApremiante_Eliminar(CodEmpresa, id);
    }
}
