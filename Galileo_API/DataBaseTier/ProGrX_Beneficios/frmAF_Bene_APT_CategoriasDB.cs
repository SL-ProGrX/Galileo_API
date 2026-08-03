using Dapper;
using Galileo.Models;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Microsoft.Data.SqlClient;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    /// <summary>
    /// Acceso a datos del catálogo de Categorías Apremiantes de Beneficios (frmAF_Bene_APT_Categorias).
    /// </summary>
    public partial class FrmAfBeneAptCategoriasDB : BeneficiosCatalogoDbBase
    {
        /// <summary>
        /// Inicializa el acceso a datos con la configuración inyectada.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmAfBeneAptCategoriasDB(IConfiguration config) : base(config)
        {
        }

        // ==========================
        // Cuerpos SQL constantes
        // ==========================

        private const string SqlCategoriasSelect = @"
SELECT id_apt_categoria, descripcion, activo, registro_fecha,
       registro_usuario, modifica_fecha, modifica_usuario
FROM AFI_BENE_APT_CATEGORIAS
";

        private const string SqlCategoriasWhere = @"
WHERE (@filtro IS NULL)
   OR (DESCRIPCION LIKE @like)
   OR (CONVERT(varchar(20), ID_APT_CATEGORIA) LIKE @like)
";

        private const string SqlCategoriasCount = @"
SELECT COUNT(1)
FROM AFI_BENE_APT_CATEGORIAS
" + SqlCategoriasWhere;

        /// <summary>
        /// Obtiene la lista de categorías apremiantes con paginación, filtro y ordenamiento.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="filtros">Filtros de carga perezosa (página, paginación, filtro y orden).</param>
        /// <returns>Lista de categorías y total de registros.</returns>
        public ErrorDto<AptCategoriasDataLista> CategoriasApremiante_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var lista = QueryCategorias(connection, filtros, true, out var total);
                return new AptCategoriasDataLista { total = total, lista = lista };
            });
        }

        /// <summary>
        /// Exporta la lista de categorías aplicando el filtro vigente, sin paginar.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="filtros">Filtros de carga perezosa; se ignora la paginación.</param>
        /// <returns>Lista de categorías sin paginar.</returns>
        public ErrorDto<List<AptCategorias>> CategoriasApremiante_Exportar(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                QueryCategorias(connection, filtros, false, out _));
        }

        // ==========================
        // Helpers privados de consulta
        // ==========================

        /// <summary>
        /// Consulta las categorías aplicando filtro, orden y, opcionalmente, paginación.
        /// </summary>
        /// <param name="connection">Conexión abierta.</param>
        /// <param name="filtros">Filtros de carga perezosa.</param>
        /// <param name="usarPaginacion">Indica si se aplica OFFSET/FETCH.</param>
        /// <param name="total">Total de registros que cumplen el filtro.</param>
        /// <returns>Lista de categorías.</returns>
        private static List<AptCategorias> QueryCategorias(
            SqlConnection connection,
            FiltrosLazyLoadData filtros,
            bool usarPaginacion,
            out int total)
        {
            var (filtro, like) = BuildFiltroLike(filtros);
            var (sortField, sortOrder) = ResolveSort(filtros);

            total = connection.QuerySingle<int>(SqlCategoriasCount, new { filtro, like });

            var sqlList = SqlCategoriasSelect + SqlCategoriasWhere + $"\nORDER BY {sortField} {sortOrder}";

            var offset = filtros?.pagina ?? 0;
            var fetch = filtros?.paginacion ?? 0;

            sqlList = AplicarPaginacion(sqlList, usarPaginacion, fetch);

            return connection.Query<AptCategorias>(sqlList, new { filtro, like, offset, fetch }).ToList();
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
                "descripcion" => "DESCRIPCION",
                "activo" => "ACTIVO",
                _ => "ID_APT_CATEGORIA"
            };

            // Convención de PrimeNG: -1 descendente, 1 ascendente (ASC por defecto).
            var sortOrder = filtros?.sortOrder == -1 ? "DESC" : "ASC";
            return (sortField, sortOrder);
        }

        /// <summary>
        /// Inserta una categoría apremiante.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="request">Datos de la categoría.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto CategoriasApremiante_Agregar(int CodEmpresa, AptCategorias request)
        {
            const string sql = @"INSERT INTO AFI_BENE_APT_CATEGORIAS (descripcion, activo, registro_fecha, registro_usuario)
                                 VALUES (@descripcion, @activo, GETDATE(), @registro_usuario)";

            var result = DbHelper.ExecuteNonQuery(CreatePortalDb(), CodEmpresa, sql, new
            {
                request.descripcion,
                activo = request.activo ? 1 : 0,
                request.registro_usuario
            });

            if (result.Code == 0)
            {
                result.Description = "Categoría agregada correctamente";
            }

            return result;
        }

        /// <summary>
        /// Actualiza una categoría apremiante existente.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="request">Datos de la categoría.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto CategoriasApremiante_Actualizar(int CodEmpresa, AptCategorias request)
        {
            const string sql = @"UPDATE AFI_BENE_APT_CATEGORIAS
                                 SET descripcion = @descripcion, activo = @activo,
                                     modifica_fecha = GETDATE(), modifica_usuario = @modifica_usuario
                                 WHERE ID_APT_CATEGORIA = @id_apt_categoria";

            var result = DbHelper.ExecuteNonQuery(CreatePortalDb(), CodEmpresa, sql, new
            {
                request.descripcion,
                activo = request.activo ? 1 : 0,
                request.modifica_usuario,
                request.id_apt_categoria
            });

            if (result.Code == 0)
            {
                result.Description = "Categoría actualizada correctamente";
            }

            return result;
        }

        /// <summary>
        /// Elimina una categoría apremiante.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="id">Identificador de la categoría.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto CategoriasApremiante_Eliminar(int CodEmpresa, int id)
        {
            const string sql = "DELETE FROM AFI_BENE_APT_CATEGORIAS WHERE ID_APT_CATEGORIA = @id";

            var result = DbHelper.ExecuteNonQuery(CreatePortalDb(), CodEmpresa, sql, new { id });

            if (result.Code == 0)
            {
                result.Description = "Categoría eliminada correctamente";
            }

            return result;
        }
    }
}
