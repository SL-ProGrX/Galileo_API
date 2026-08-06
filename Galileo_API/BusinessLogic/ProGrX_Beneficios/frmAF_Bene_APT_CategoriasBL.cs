using Galileo.DataBaseTier.ProGrX_Beneficios;
using Galileo.Models;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Newtonsoft.Json;

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

        /// <summary>Lista de categorías apremiantes con paginación, filtro y ordenamiento.</summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="filtros">Filtros de carga perezosa serializados en JSON.</param>
        public ErrorDto<AptCategoriasDataLista> CategoriasApremiante_Obtener(int CodEmpresa, string? filtros)
            => _db.CategoriasApremiante_Obtener(CodEmpresa, DeserializarFiltros(filtros));

        /// <summary>Exporta la lista de categorías aplicando el filtro vigente, sin paginar.</summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="filtros">Filtros de carga perezosa serializados en JSON.</param>
        public ErrorDto<List<AptCategorias>> CategoriasApremiante_Exportar(int CodEmpresa, string? filtros)
            => _db.CategoriasApremiante_Exportar(CodEmpresa, DeserializarFiltros(filtros));

        /// <summary>
        /// Convierte el JSON de filtros recibido desde Angular en el modelo de carga perezosa.
        /// </summary>
        /// <param name="filtros">Filtros serializados en JSON.</param>
        /// <returns>Filtros deserializados; instancia vacía si el JSON viene nulo o inválido.</returns>
        private static FiltrosLazyLoadData DeserializarFiltros(string? filtros)
        {
            if (string.IsNullOrWhiteSpace(filtros))
            {
                return new FiltrosLazyLoadData();
            }

            return JsonConvert.DeserializeObject<FiltrosLazyLoadData>(filtros) ?? new FiltrosLazyLoadData();
        }

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
