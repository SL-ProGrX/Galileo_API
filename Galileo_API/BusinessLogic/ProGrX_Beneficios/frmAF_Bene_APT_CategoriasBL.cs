using Galileo.DataBaseTier.ProGrX_Beneficios;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo_API.BusinessLogic.ProGrX_Beneficios
{
    /// <summary>
    /// Lógica de negocio del catálogo de Categorías Apremiantes de Beneficios (frmAF_Bene_APT_Categorias).
    /// </summary>
    public class FrmAfBeneAptCategoriasBL
    {
        private readonly FrmAfBeneAptCategoriasDB _db;

        public FrmAfBeneAptCategoriasBL(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _db = new FrmAfBeneAptCategoriasDB(config);
        }

        /// <summary>Lista de categorías apremiantes.</summary>
        public ErrorDto<AptCategoriasDataLista> CategoriasApremiante_Obtener(int CodEmpresa, int? pagina, int? paginacion, string? filtro)
            => _db.CategoriasApremiante_Obtener(CodEmpresa, pagina, paginacion, filtro);

        /// <summary>Inserta una categoría apremiante.</summary>
        public ErrorDto CategoriasApremiante_Agregar(int CodEmpresa, AptCategorias request)
            => _db.CategoriasApremiante_Agregar(CodEmpresa, request);

        /// <summary>Actualiza una categoría apremiante.</summary>
        public ErrorDto CategoriasApremiante_Actualizar(int CodEmpresa, AptCategorias request)
            => _db.CategoriasApremiante_Actualizar(CodEmpresa, request);

        /// <summary>Elimina una categoría apremiante.</summary>
        public ErrorDto CategoriasApremiante_Eliminar(int CodEmpresa, int id)
            => _db.CategoriasApremiante_Eliminar(CodEmpresa, id);
    }
}
