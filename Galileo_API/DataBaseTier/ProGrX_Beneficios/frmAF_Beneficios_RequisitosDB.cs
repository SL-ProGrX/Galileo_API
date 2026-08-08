using Dapper;
using Galileo.Models;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Microsoft.Data.SqlClient;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    /// <summary>
    /// Acceso a datos del catálogo de Requisitos para Beneficios (frmAF_Beneficios_Requisitos).
    /// Consultas aquí; guardado en el parcial .Guardar.
    /// </summary>
    public partial class FrmAfBeneficiosRequisitosDB : BeneficiosCatalogoDbBase
    {
        /// <summary>
        /// Inicializa el acceso a datos con la configuración inyectada.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmAfBeneficiosRequisitosDB(IConfiguration config) : base(config)
        {
        }

        // ==========================
        // Cuerpos SQL constantes
        // ==========================

        private const string SqlRequisitosSelect = @"
SELECT COD_REQUISITO AS cod_requisito, descripcion, Activo AS activo, requerido
FROM AFI_BENE_REQUISITOS
";

        private const string SqlRequisitosWhere = @"
WHERE (@filtro IS NULL)
   OR (COD_REQUISITO LIKE @like)
   OR (descripcion LIKE @like)
";

        private const string SqlRequisitosCount = @"
SELECT COUNT(1)
FROM AFI_BENE_REQUISITOS
" + SqlRequisitosWhere;

        /// <summary>
        /// Obtiene la lista de requisitos para beneficios con paginación, filtro y ordenamiento.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="filtros">Filtros de carga perezosa (página, paginación, filtro y orden).</param>
        /// <returns>Lista de requisitos y total de registros.</returns>
        public ErrorDto<BeneRequisitosDataLista> AfBeneRequisitos_Obtener(int CodCliente, FiltrosLazyLoadData filtros)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var lista = QueryRequisitos(connection, filtros, true, out var total);
                return new BeneRequisitosDataLista { total = total, lista = lista };
            });
        }

        /// <summary>
        /// Exporta la lista de requisitos aplicando el filtro vigente, sin paginar.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="filtros">Filtros de carga perezosa; se ignora la paginación.</param>
        /// <returns>Lista de requisitos sin paginar.</returns>
        public ErrorDto<List<BeneRequisitosData>> AfBeneRequisitos_Exportar(int CodCliente, FiltrosLazyLoadData filtros)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                QueryRequisitos(connection, filtros, false, out _));
        }

        // ==========================
        // Helpers privados de consulta
        // ==========================

        /// <summary>
        /// Consulta los requisitos aplicando filtro, orden y, opcionalmente, paginación.
        /// </summary>
        /// <param name="connection">Conexión abierta.</param>
        /// <param name="filtros">Filtros de carga perezosa.</param>
        /// <param name="usarPaginacion">Indica si se aplica OFFSET/FETCH.</param>
        /// <param name="total">Total de registros que cumplen el filtro.</param>
        /// <returns>Lista de requisitos.</returns>
        private static List<BeneRequisitosData> QueryRequisitos(
            SqlConnection connection,
            FiltrosLazyLoadData filtros,
            bool usarPaginacion,
            out int total)
        {
            var (filtro, like) = BuildFiltroLike(filtros);
            var (sortField, sortOrder) = ResolveSort(filtros);

            total = connection.QuerySingle<int>(SqlRequisitosCount, new { filtro, like });

            var sqlList = SqlRequisitosSelect + SqlRequisitosWhere + $"\nORDER BY {sortField} {sortOrder}";

            var offset = filtros?.pagina ?? 0;
            var fetch = filtros?.paginacion ?? 0;

            sqlList = AplicarPaginacion(sqlList, usarPaginacion, fetch);

            return connection.Query<BeneRequisitosData>(sqlList, new { filtro, like, offset, fetch }).ToList();
        }

        /// <summary>
        /// Resuelve el campo y la dirección de ordenamiento usando una lista blanca de columnas.
        /// </summary>
        /// <param name="filtros">Filtros de carga perezosa.</param>
        /// <returns>Tupla con el campo y la dirección de ordenamiento.</returns>
        private static (string sortField, string sortOrder) ResolveSort(FiltrosLazyLoadData filtros)
        {
            // ORDER BY seguro (whitelist), nunca se concatena texto recibido del usuario.
            var sortField = (filtros?.sortField ?? string.Empty).Trim().ToLowerInvariant() switch
            {
                "descripcion" => "descripcion",
                "requerido" => "requerido",
                "activo" => "Activo",
                _ => "COD_REQUISITO"
            };

            // Convención de PrimeNG: -1 descendente, 1 ascendente (ASC por defecto).
            var sortOrder = filtros?.sortOrder == -1 ? "DESC" : "ASC";
            return (sortField, sortOrder);
        }
    }
}
