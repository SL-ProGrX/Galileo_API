using Dapper;
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

        /// <summary>
        /// Obtiene la lista de grupos de beneficios con paginación y filtro.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="pagina">Offset de paginación.</param>
        /// <param name="paginacion">Cantidad de registros por página.</param>
        /// <param name="filtro">Filtro por código o descripción.</param>
        /// <returns>Lista de grupos y total.</returns>
        public ErrorDto<BeneficioGrupoDataLista> BeneficioGrupoLista_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var response = new BeneficioGrupoDataLista();

                const string sqlCount = "SELECT COUNT(*) FROM AFI_BENEFICIO_GRUPOS";
                response.Total = connection.QueryFirstOrDefault<int>(sqlCount);

                var like = string.IsNullOrWhiteSpace(filtro) ? null : $"%{filtro}%";
                var offset = pagina ?? 0;
                var fetch = paginacion ?? 10;

                const string sql = @"SELECT cod_grupo, descripcion FROM AFI_BENEFICIO_GRUPOS
                                     WHERE (@like IS NULL OR cod_grupo LIKE @like OR descripcion LIKE @like)
                                     ORDER BY cod_grupo
                                     OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY";

                response.beneficios = connection.Query<BeneficioGrupoData>(sql, new { like, offset, fetch }).ToList();
                return response;
            });
        }

        /// <summary>
        /// Obtiene la lista de usuarios y su pertenencia a un grupo de beneficios.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="pagina">Offset de paginación.</param>
        /// <param name="paginacion">Cantidad de registros por página.</param>
        /// <param name="filtro">Filtro por nombre o descripción.</param>
        /// <param name="cod_grupo">Código del grupo.</param>
        /// <returns>Lista de usuarios y total.</returns>
        public ErrorDto<BeneficioUsuariosDataLista> BeneficioUsuariosLista_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro, string cod_grupo)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var response = new BeneficioUsuariosDataLista();

                const string sqlCount = @"SELECT COUNT(*)
                                          FROM Usuarios U
                                          LEFT JOIN AFI_BENE_USERG A ON U.nombre = A.usuario AND A.cod_grupo = @cod_grupo
                                          WHERE U.estado = 'A'";
                response.Total = connection.QueryFirstOrDefault<int>(sqlCount, new { cod_grupo });

                var like = string.IsNullOrWhiteSpace(filtro) ? null : $"%{filtro}%";
                var offset = pagina ?? 0;
                var fetch = paginacion ?? 10;

                // Se conserva la precedencia original del filtro (sin paréntesis) para no alterar el comportamiento VB6.
                const string sql = @"SELECT U.nombre, U.descripcion, A.usuario,
                                            CASE WHEN A.usuario IS NULL THEN 0 ELSE 1 END AS activo
                                     FROM Usuarios U
                                     LEFT JOIN AFI_BENE_USERG A ON U.nombre = A.usuario AND A.cod_grupo = @cod_grupo
                                     WHERE U.estado = 'A'
                                       AND (@like IS NULL OR U.nombre LIKE @like OR U.descripcion LIKE @like)
                                     ORDER BY A.usuario DESC, U.nombre ASC
                                     OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY";

                response.usuarios = connection.Query<BeneficioUsuariosData>(sql, new { cod_grupo, like, offset, fetch }).ToList();
                return response;
            });
        }

        /// <summary>
        /// Obtiene la lista completa de grupos de beneficios.
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
