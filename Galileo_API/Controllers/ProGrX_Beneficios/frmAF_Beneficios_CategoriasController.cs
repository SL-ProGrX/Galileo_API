using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Beneficios;

namespace Galileo_API.Controllers.ProGrX_Beneficios
{
    /// <summary>
    /// Endpoints de Categorías de Beneficios (frmAF_Beneficios_Categorias).
    /// </summary>
    [Route("api/frmAF_Beneficios_Categorias")]
    [ApiController]
    public class FrmAfBeneficiosCategoriasController : ControllerBase
    {
        private readonly FrmAfBeneficiosCategoriasBL _bl;

        public FrmAfBeneficiosCategoriasController(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _bl = new FrmAfBeneficiosCategoriasBL(config);
        }

        /// <summary>Lista de categorías de beneficios.</summary>
        [Authorize]
        [HttpGet("BeneficiosCategorias_Obtener")]
        public ErrorDto<BEeneCategoriaDataLista> BeneficiosCategorias_Obtener(int CodEmpresa, int? pagina, int? paginacion, string? filtro)
            => _bl.BeneficiosCategorias_Obtener(CodEmpresa, pagina, paginacion, filtro);

        /// <summary>Permisos por usuario de una categoría.</summary>
        [Authorize]
        [HttpGet("BeneficiosCategorias_ObtenerPermisos")]
        public ErrorDto<List<BeneCategoriaPermisos>> BeneficiosCategorias_ObtenerPermisos(int CodCliente, string cod_categoria, string? filtro)
            => _bl.BeneficiosCategorias_ObtenerPermisos(CodCliente, cod_categoria, filtro);

        /// <summary>Catálogo de validaciones de beneficios activas.</summary>
        [Authorize]
        [HttpGet("BeneValidacionesLista_Obtener")]
        public ErrorDto<List<BeneValidaLista>> BeneValidacionesLista_Obtener(int CodCliente)
            => _bl.BeneValidacionesLista_Obtener(CodCliente);

        /// <summary>Validaciones asignadas a una categoría.</summary>
        [Authorize]
        [HttpGet("BeneCategoriaValida_Obtener")]
        public ErrorDto<List<BeneCategoriaValidaLista>> BeneCategoriaValida_Obtener(int CodCliente, string cod_categoria)
            => _bl.BeneCategoriaValida_Obtener(CodCliente, cod_categoria);

        /// <summary>Inserta una categoría de beneficios.</summary>
        [Authorize]
        [HttpPost("BeneficiosCategorias_Agregar")]
        public ErrorDto BeneficiosCategorias_Agregar(int CodEmpresa, [FromBody] BeneCategoria request)
            => _bl.BeneficiosCategorias_Agregar(CodEmpresa, request);

        /// <summary>Actualiza una categoría de beneficios.</summary>
        [Authorize]
        [HttpPut("BeneficiosCategorias_Actualizar")]
        public ErrorDto BeneficiosCategorias_Actualizar(int CodEmpresa, [FromBody] BeneCategoria request)
            => _bl.BeneficiosCategorias_Actualizar(CodEmpresa, request);

        /// <summary>Elimina una categoría de beneficios.</summary>
        [Authorize]
        [HttpDelete("BeneficiosCategorias_Eliminar")]
        public ErrorDto BeneficiosCategorias_Eliminar(int CodEmpresa, string id)
            => _bl.BeneficiosCategorias_Eliminar(CodEmpresa, id);

        /// <summary>Guarda una validación de categoría (inserta o actualiza).</summary>
        [Authorize]
        [HttpPost("BeneCategoriaValida_Guardar")]
        public ErrorDto BeneCategoriaValida_Guardar(int CodCliente, [FromBody] BeneCategoriaValidaLista valida)
            => _bl.BeneCategoriaValida_Guardar(CodCliente, valida);

        /// <summary>Registra los permisos de un usuario en una categoría.</summary>
        [Authorize]
        [HttpPost("registroPermisosCategoria")]
        public ErrorDto registroPermisosCategoria(int CodCliente, string Cod_Categoria, [FromBody] BeneCategoriaPermisos request)
            => _bl.registroPermisosCategoria(CodCliente, Cod_Categoria, request);
    }
}
