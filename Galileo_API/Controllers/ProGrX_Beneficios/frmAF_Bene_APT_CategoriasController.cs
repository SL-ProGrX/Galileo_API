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

        /// <summary>Lista de categorías apremiantes.</summary>
        [Authorize]
        [HttpGet("CategoriasApremiante_Obtener")]
        public ErrorDto<AptCategoriasDataLista> CategoriasApremiante_Obtener(int CodEmpresa, int? pagina, int? paginacion, string? filtro)
            => _bl.CategoriasApremiante_Obtener(CodEmpresa, pagina, paginacion, filtro);

        /// <summary>Inserta una categoría apremiante.</summary>
        [Authorize]
        [HttpPost("CategoriasApremiante_Agregar")]
        public ErrorDto CategoriasApremiante_Agregar(int CodEmpresa, [FromBody] AptCategorias request)
            => _bl.CategoriasApremiante_Agregar(CodEmpresa, request);

        /// <summary>Actualiza una categoría apremiante.</summary>
        [Authorize]
        [HttpPut("CategoriasApremiante_Actualizar")]
        public ErrorDto CategoriasApremiante_Actualizar(int CodEmpresa, [FromBody] AptCategorias request)
            => _bl.CategoriasApremiante_Actualizar(CodEmpresa, request);

        /// <summary>Elimina una categoría apremiante.</summary>
        [Authorize]
        [HttpDelete("CategoriasApremiante_Eliminar")]
        public ErrorDto CategoriasApremiante_Eliminar(int CodEmpresa, int id)
            => _bl.CategoriasApremiante_Eliminar(CodEmpresa, id);
    }
}
