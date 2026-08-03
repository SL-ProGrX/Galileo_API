using Galileo.Models;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    /// <summary>
    /// Base común para el acceso a datos de los catálogos de Beneficios (Grupos, Productos,
    /// Estados, Requisitos, Motivos, Roles, Bancos y Categorías/Profesionales Apremiantes).
    /// Centraliza la configuración, la creación de conexión al portal y los helpers de
    /// filtro/paginación que se repetían de forma idéntica en cada catálogo.
    /// </summary>
    public abstract class BeneficiosCatalogoDbBase
    {
        protected readonly IConfiguration _config;

        /// <summary>
        /// Inicializa el acceso a datos con la configuración inyectada.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        protected BeneficiosCatalogoDbBase(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        protected PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Construye el texto de filtro y su patrón LIKE. Devuelve nulos cuando no hay filtro.
        /// </summary>
        /// <param name="filtros">Filtros de carga perezosa.</param>
        /// <returns>Tupla con el filtro normalizado y su patrón LIKE.</returns>
        protected static (string? filtro, string? like) BuildFiltroLike(FiltrosLazyLoadData filtros)
        {
            var texto = filtros?.filtro?.Trim();
            if (string.IsNullOrWhiteSpace(texto))
            {
                return (null, null);
            }

            return (texto, $"%{texto}%");
        }

        /// <summary>
        /// Agrega OFFSET/FETCH al listado cuando se solicita paginación, o el cierre simple del
        /// statement en caso contrario (por ejemplo, al exportar sin paginar).
        /// </summary>
        /// <param name="sqlList">Consulta base ya ordenada, sin el cierre final.</param>
        /// <param name="usarPaginacion">Indica si se aplica OFFSET/FETCH.</param>
        /// <param name="fetch">Cantidad de registros por página.</param>
        /// <returns>Consulta con la paginación o el cierre aplicado.</returns>
        protected static string AplicarPaginacion(string sqlList, bool usarPaginacion, int fetch)
        {
            return usarPaginacion && fetch > 0
                ? sqlList + "\nOFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY;"
                : sqlList + ";";
        }
    }
}
