using Dapper;
using Galileo.Models;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Microsoft.Data.SqlClient;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    /// <summary>
    /// Acceso a datos de Roles/Grupos de Beneficios (frmAF_BeneficioRoles).
    /// Consultas y helpers aquí; guardado en el parcial .Guardar.
    /// </summary>
    public partial class FrmAfBeneficioRolesDB
    {
        private readonly IConfiguration _config;

        /// <summary>
        /// Inicializa el acceso a datos con la configuración inyectada.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmAfBeneficioRolesDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        private PortalDB CreatePortalDb() => new(_config);

        // ==========================
        // Cuerpos SQL constantes
        // ==========================

        private const string SqlGruposSelect = @"
SELECT cod_grupo, descripcion
FROM AFI_BENEFICIO_GRUPOS
";

        private const string SqlGruposWhere = @"
WHERE (@filtro IS NULL)
   OR (cod_grupo LIKE @like)
   OR (descripcion LIKE @like)
";

        private const string SqlGruposCount = @"
SELECT COUNT(1)
FROM AFI_BENEFICIO_GRUPOS
" + SqlGruposWhere;

        private const string SqlUsuariosFrom = @"
FROM Usuarios U
LEFT JOIN AFI_BENE_USERG A ON U.nombre = A.usuario AND A.cod_grupo = @cod_grupo
WHERE U.estado = 'A'
  AND ((@filtro IS NULL) OR (U.nombre LIKE @like) OR (U.descripcion LIKE @like))
";

        private const string SqlUsuariosCount = "SELECT COUNT(1) " + SqlUsuariosFrom;

        private const string SqlUsuariosSelect = @"
SELECT U.nombre, U.descripcion, A.usuario,
       CASE WHEN A.usuario IS NULL THEN 0 ELSE 1 END AS activo
" + SqlUsuariosFrom;

        // ==========================
        // Consultas públicas
        // ==========================

        /// <summary>
        /// Obtiene la lista de grupos de beneficios con paginación, filtro y ordenamiento.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="filtros">Filtros de carga perezosa (página, paginación, filtro y orden).</param>
        /// <returns>Lista de grupos y total de registros.</returns>
        public ErrorDto<BeneficioGrupoDataLista> BeneficioGrupoLista_Obtener(int CodCliente, FiltrosLazyLoadData filtros)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var lista = QueryGrupos(connection, filtros, true, out var total);
                return new BeneficioGrupoDataLista { total = total, beneficios = lista };
            });
        }

        /// <summary>
        /// Exporta la lista de grupos aplicando el filtro vigente, sin paginar.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="filtros">Filtros de carga perezosa; se ignora la paginación.</param>
        /// <returns>Lista de grupos sin paginar.</returns>
        public ErrorDto<List<BeneficioGrupoData>> BeneficioGrupo_Exportar(int CodCliente, FiltrosLazyLoadData filtros)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                QueryGrupos(connection, filtros, false, out _));
        }

        /// <summary>
        /// Obtiene la lista de usuarios y su pertenencia a un grupo de beneficios.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="cod_grupo">Código del grupo.</param>
        /// <param name="filtros">Filtros de carga perezosa (página, paginación, filtro y orden).</param>
        /// <returns>Lista de usuarios y total de registros.</returns>
        public ErrorDto<BeneficioUsuariosDataLista> BeneficioUsuariosLista_Obtener(
            int CodCliente, string cod_grupo, FiltrosLazyLoadData filtros)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var (filtro, like) = BuildFiltroLike(filtros);
                var (sortField, sortOrder) = ResolveSortUsuarios(filtros);

                var parametros = new
                {
                    cod_grupo,
                    filtro,
                    like,
                    offset = filtros?.pagina ?? 0,
                    fetch = filtros?.paginacion ?? 0,
                };

                var total = connection.QuerySingle<int>(SqlUsuariosCount, parametros);

                var sqlList = SqlUsuariosSelect + $"\nORDER BY {sortField} {sortOrder}";
                sqlList += parametros.fetch > 0
                    ? "\nOFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY;"
                    : ";";

                var lista = connection.Query<BeneficioUsuariosData>(sqlList, parametros).ToList();
                return new BeneficioUsuariosDataLista { total = total, usuarios = lista };
            });
        }

        /// <summary>
        /// Obtiene la lista completa de grupos de beneficios, usada para alimentar el selector de asignación.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <returns>Lista de grupos.</returns>
        public ErrorDto<List<BeneficioGrupoData>> BeneficioGrupoData_Obtener(int CodCliente)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = "SELECT cod_grupo, descripcion FROM AFI_BENEFICIO_GRUPOS ORDER BY cod_grupo";
                return connection.Query<BeneficioGrupoData>(sql).ToList();
            });
        }

        // ==========================
        // Helpers privados
        // ==========================

        /// <summary>
        /// Consulta los grupos aplicando filtro, orden y, opcionalmente, paginación.
        /// </summary>
        /// <param name="connection">Conexión abierta.</param>
        /// <param name="filtros">Filtros de carga perezosa.</param>
        /// <param name="usarPaginacion">Indica si se aplica OFFSET/FETCH.</param>
        /// <param name="total">Total de registros que cumplen el filtro.</param>
        /// <returns>Lista de grupos.</returns>
        private static List<BeneficioGrupoData> QueryGrupos(
            SqlConnection connection,
            FiltrosLazyLoadData filtros,
            bool usarPaginacion,
            out int total)
        {
            var (filtro, like) = BuildFiltroLike(filtros);
            var (sortField, sortOrder) = ResolveSortGrupos(filtros);

            total = connection.QuerySingle<int>(SqlGruposCount, new { filtro, like });

            var sqlList = SqlGruposSelect + SqlGruposWhere + $"\nORDER BY {sortField} {sortOrder}";

            var offset = filtros?.pagina ?? 0;
            var fetch = filtros?.paginacion ?? 0;

            if (usarPaginacion && fetch > 0)
            {
                sqlList += "\nOFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY;";
            }
            else
            {
                sqlList += ";";
            }

            return connection.Query<BeneficioGrupoData>(sqlList, new { filtro, like, offset, fetch }).ToList();
        }

        /// <summary>
        /// Construye el texto de filtro y su patrón LIKE. Devuelve nulos cuando no hay filtro.
        /// </summary>
        /// <param name="filtros">Filtros de carga perezosa.</param>
        /// <returns>Tupla con el filtro normalizado y su patrón LIKE.</returns>
        private static (string? filtro, string? like) BuildFiltroLike(FiltrosLazyLoadData filtros)
        {
            var texto = filtros?.filtro?.Trim();
            if (string.IsNullOrWhiteSpace(texto))
            {
                return (null, null);
            }

            return (texto, $"%{texto}%");
        }

        /// <summary>
        /// Resuelve el ordenamiento de la tabla de grupos usando una lista blanca de columnas.
        /// </summary>
        /// <param name="filtros">Filtros de carga perezosa.</param>
        /// <returns>Tupla con el campo y la dirección de ordenamiento.</returns>
        private static (string sortField, string sortOrder) ResolveSortGrupos(FiltrosLazyLoadData filtros)
        {
            // ORDER BY seguro (whitelist), nunca se concatena texto recibido del usuario.
            var sortField = (filtros?.sortField ?? string.Empty).Trim().ToLowerInvariant() switch
            {
                "descripcion" => "descripcion",
                _ => "cod_grupo"
            };

            // Convención de PrimeNG: -1 descendente, 1 ascendente (ASC por defecto).
            var sortOrder = filtros?.sortOrder == -1 ? "DESC" : "ASC";
            return (sortField, sortOrder);
        }

        /// <summary>
        /// Resuelve el ordenamiento de la tabla de usuarios usando una lista blanca de columnas.
        /// Por defecto se conserva el orden original: primero los usuarios ya asignados.
        /// </summary>
        /// <param name="filtros">Filtros de carga perezosa.</param>
        /// <returns>Tupla con el campo y la dirección de ordenamiento.</returns>
        private static (string sortField, string sortOrder) ResolveSortUsuarios(FiltrosLazyLoadData filtros)
        {
            var campo = (filtros?.sortField ?? string.Empty).Trim().ToLowerInvariant();

            if (campo.Length == 0)
            {
                return ("A.usuario DESC, U.nombre", "ASC");
            }

            // ORDER BY seguro (whitelist), nunca se concatena texto recibido del usuario.
            var sortField = campo switch
            {
                "descripcion" => "U.descripcion",
                "activo" => "A.usuario",
                _ => "U.nombre"
            };

            // Convención de PrimeNG: -1 descendente, 1 ascendente (ASC por defecto).
            var sortOrder = filtros?.sortOrder == -1 ? "DESC" : "ASC";
            return (sortField, sortOrder);
        }

        /// <summary>
        /// Verifica si un grupo de beneficios existe.
        /// </summary>
        /// <param name="connection">Conexión abierta.</param>
        /// <param name="cod_grupo">Código del grupo.</param>
        /// <returns>True si existe.</returns>
        private static bool BeneficioGrupo_Existe(SqlConnection connection, string cod_grupo)
        {
            const string sql = "SELECT COUNT(*) FROM AFI_BENEFICIO_GRUPOS WHERE cod_grupo = @cod_grupo";
            return connection.QueryFirstOrDefault<int>(sql, new { cod_grupo }) > 0;
        }
    }
}
