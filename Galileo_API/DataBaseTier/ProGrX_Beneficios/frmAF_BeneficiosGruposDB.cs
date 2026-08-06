using Dapper;
using Galileo.Models;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Microsoft.Data.SqlClient;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    /// <summary>
    /// Acceso a datos de Grupos de Beneficios (frmAF_BeneficiosGrupos).
    /// Consultas aquí; guardado en .Guardar, asignaciones por SP en .Asignaciones.
    /// </summary>
    public partial class FrmAfBeneficiosGruposDB : BeneficiosCatalogoDbBase
    {
        private readonly MBeneficiosDB _mBeneficiosDB;

        /// <summary>
        /// Inicializa el acceso a datos y la bitácora de beneficios con la configuración inyectada.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmAfBeneficiosGruposDB(IConfiguration config) : base(config)
        {
            _mBeneficiosDB = new MBeneficiosDB(_config);
        }

        // ==========================
        // Cuerpos SQL constantes
        // ==========================

        private const string SqlGruposSelect = @"
SELECT cod_grupo, descripcion, Cod_Categoria AS cod_categoria, monto, estado,
       User_Registra AS user_registra, Fecha AS fecha
FROM AFI_BENE_GRUPOS
";

        private const string SqlGruposWhere = @"
WHERE (@filtro IS NULL)
   OR (cod_grupo LIKE @like)
   OR (descripcion LIKE @like)
   OR (Cod_Categoria LIKE @like)
";

        private const string SqlGruposCount = @"
SELECT COUNT(1)
FROM AFI_BENE_GRUPOS
" + SqlGruposWhere;

        /// <summary>
        /// Obtiene la lista de grupos de beneficios con paginación, filtro y ordenamiento.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="filtros">Filtros de carga perezosa (página, paginación, filtro y orden).</param>
        /// <returns>Lista de grupos y total de registros.</returns>
        public ErrorDto<AfiBeneGruposLista> AfiBeneGrupos_Obtener(int CodCliente, FiltrosLazyLoadData filtros)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var lista = QueryGrupos(connection, filtros, true, out var total);
                return new AfiBeneGruposLista { total = total, beneficios = lista };
            });
        }

        // ==========================
        // Helpers privados de consulta
        // ==========================

        /// <summary>
        /// Consulta los grupos aplicando filtro, orden y, opcionalmente, paginación.
        /// </summary>
        /// <param name="connection">Conexión abierta.</param>
        /// <param name="filtros">Filtros de carga perezosa.</param>
        /// <param name="usarPaginacion">Indica si se aplica OFFSET/FETCH.</param>
        /// <param name="total">Total de registros que cumplen el filtro.</param>
        /// <returns>Lista de grupos.</returns>
        private static List<AfiBeneGrupos> QueryGrupos(
            SqlConnection connection,
            FiltrosLazyLoadData filtros,
            bool usarPaginacion,
            out int total)
        {
            var (filtro, like) = BuildFiltroLike(filtros);
            var (sortField, sortOrder) = ResolveSort(filtros);

            total = connection.QuerySingle<int>(SqlGruposCount, new { filtro, like });

            var sqlList = SqlGruposSelect + SqlGruposWhere + $"\nORDER BY {sortField} {sortOrder}";

            var offset = filtros?.pagina ?? 0;
            var fetch = filtros?.paginacion ?? 0;

            sqlList = AplicarPaginacion(sqlList, usarPaginacion, fetch);

            return connection.Query<AfiBeneGrupos>(sqlList, new { filtro, like, offset, fetch }).ToList();
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
                "cod_categoria" => "Cod_Categoria",
                "monto" => "monto",
                "estado" => "estado",
                _ => "cod_grupo"
            };

            // Convención de PrimeNG: -1 descendente, 1 ascendente (ASC por defecto).
            var sortOrder = filtros?.sortOrder == -1 ? "DESC" : "ASC";
            return (sortField, sortOrder);
        }

        /// <summary>
        /// Obtiene la lista de beneficios y su marca de asignación a un grupo.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="pagina">Offset de paginación.</param>
        /// <param name="paginacion">Cantidad de registros por página.</param>
        /// <param name="filtro">Filtro por código o descripción del beneficio.</param>
        /// <param name="cod_grupo">Código del grupo.</param>
        /// <returns>Lista de beneficios asignados y total.</returns>
        public ErrorDto<AfiBeneGruposAsigandosLista> BeneficioUsuariosLista_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro, string cod_grupo)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var response = new AfiBeneGruposAsigandosLista();

                const string sqlCount = @"SELECT COUNT(*)
                                          FROM afi_beneficios B
                                          LEFT JOIN afi_Grupo_Beneficio G ON B.Cod_beneficio = G.cod_beneficio AND G.cod_grupo = @cod_grupo";
                response.Total = connection.QueryFirstOrDefault<int>(sqlCount, new { cod_grupo });

                var like = string.IsNullOrWhiteSpace(filtro) ? null : $"%{filtro}%";
                var offset = pagina ?? 0;
                var fetch = paginacion ?? 10;

                const string sql = @"SELECT B.cod_beneficio, B.descripcion,
                                            ISNULL(G.Cod_grupo, -1) AS marca,
                                            CASE WHEN G.Cod_grupo IS NULL THEN 0 ELSE 1 END AS activo
                                     FROM afi_beneficios B
                                     LEFT JOIN afi_Grupo_Beneficio G ON B.Cod_beneficio = G.cod_beneficio AND G.cod_grupo = @cod_grupo
                                     WHERE (@like IS NULL OR B.cod_beneficio LIKE @like OR B.descripcion LIKE @like)
                                     ORDER BY G.cod_beneficio DESC, B.descripcion
                                     OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY";

                response.beneficios = connection.Query<AfiBeneGruposAsigandosData>(sql, new { cod_grupo, like, offset, fetch }).ToList();
                return response;
            });
        }

        /// <summary>
        /// Obtiene la lista simple de grupos de beneficios.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <returns>Lista de grupos.</returns>
        public ErrorDto<List<AfiBeneGrupos>> AfiBeneGrupos_lista(int CodCliente)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = "SELECT cod_grupo, descripcion, monto, estado FROM AFI_BENE_GRUPOS";
                return connection.Query<AfiBeneGrupos>(sql).ToList();
            });
        }

        /// <summary>
        /// Obtiene el catálogo de categorías de beneficios activas.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <returns>Lista de categorías.</returns>
        public ErrorDto<List<AfiBeneLista>> AfiBeneCategoriaLista_Obtener(int CodCliente)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = @"SELECT RTRIM(COD_CATEGORIA) AS item,
                                            RTRIM(COD_CATEGORIA) + ' ' + RTRIM(DESCRIPCION) AS descripcion
                                     FROM AFI_BENE_CATEGORIAS WHERE Activo = 1 ORDER BY COD_CATEGORIA";
                return connection.Query<AfiBeneLista>(sql).ToList();
            });
        }

        /// <summary>
        /// Exporta la lista de grupos de beneficios aplicando el filtro vigente, sin paginar.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="filtros">Filtros de carga perezosa; se ignora la paginación.</param>
        /// <returns>Lista de grupos sin paginar.</returns>
        public ErrorDto<List<AfiBeneGrupos>> AfiBeneGrupoExportar(int CodCliente, FiltrosLazyLoadData filtros)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                QueryGrupos(connection, filtros, false, out _));
        }
    }
}
