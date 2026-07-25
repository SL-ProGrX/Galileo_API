using Galileo.DataBaseTier.ProGrX_Beneficios;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo_API.BusinessLogic.ProGrX_Beneficios
{
    /// <summary>
    /// Lógica de negocio de Categorías de Beneficios (frmAF_Beneficios_Categorias).
    /// </summary>
    public class FrmAfBeneficiosCategoriasBL
    {
        private readonly FrmAfBeneficiosCategoriasDB _db;

        public FrmAfBeneficiosCategoriasBL(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _db = new FrmAfBeneficiosCategoriasDB(config);
        }

        /// <summary>Lista de categorías de beneficios.</summary>
        public ErrorDto<BEeneCategoriaDataLista> BeneficiosCategorias_Obtener(int CodEmpresa, int? pagina, int? paginacion, string? filtro)
            => _db.BeneficiosCategorias_Obtener(CodEmpresa, pagina, paginacion, filtro);

        /// <summary>Permisos por usuario de una categoría.</summary>
        public ErrorDto<List<BeneCategoriaPermisos>> BeneficiosCategorias_ObtenerPermisos(int CodCliente, string cod_categoria, string? filtro)
            => _db.BeneficiosCategorias_ObtenerPermisos(CodCliente, cod_categoria, filtro);

        /// <summary>Catálogo de validaciones de beneficios activas.</summary>
        public ErrorDto<List<BeneValidaLista>> BeneValidacionesLista_Obtener(int CodCliente)
            => _db.BeneValidacionesLista_Obtener(CodCliente);

        /// <summary>Validaciones asignadas a una categoría.</summary>
        public ErrorDto<List<BeneCategoriaValidaLista>> BeneCategoriaValida_Obtener(int CodCliente, string cod_categoria)
            => _db.BeneCategoriaValida_Obtener(CodCliente, cod_categoria);

        /// <summary>Inserta una categoría de beneficios.</summary>
        public ErrorDto BeneficiosCategorias_Agregar(int CodEmpresa, BeneCategoria request)
            => _db.BeneficiosCategorias_Agregar(CodEmpresa, request);

        /// <summary>Actualiza una categoría de beneficios.</summary>
        public ErrorDto BeneficiosCategorias_Actualizar(int CodEmpresa, BeneCategoria request)
            => _db.BeneficiosCategorias_Actualizar(CodEmpresa, request);

        /// <summary>Elimina una categoría de beneficios.</summary>
        public ErrorDto BeneficiosCategorias_Eliminar(int CodEmpresa, string id)
            => _db.BeneficiosCategorias_Eliminar(CodEmpresa, id);

        /// <summary>Guarda una validación de categoría (inserta o actualiza).</summary>
        public ErrorDto BeneCategoriaValida_Guardar(int CodCliente, BeneCategoriaValidaLista valida)
            => _db.BeneCategoriaValida_Guardar(CodCliente, valida);

        /// <summary>Registra los permisos de un usuario en una categoría.</summary>
        public ErrorDto registroPermisosCategoria(int CodCliente, string Cod_Categoria, BeneCategoriaPermisos request)
            => _db.registroPermisosCategoria(CodCliente, Cod_Categoria, request);
    }
}
