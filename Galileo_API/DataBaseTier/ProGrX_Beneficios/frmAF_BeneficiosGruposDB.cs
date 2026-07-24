using Dapper;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    /// <summary>
    /// Acceso a datos de Grupos de Beneficios (frmAF_BeneficiosGrupos).
    /// Consultas aquí; guardado en .Guardar, asignaciones por SP en .Asignaciones.
    /// </summary>
    public partial class FrmAfBeneficiosGruposDB
    {
        private readonly IConfiguration _config;
        private readonly MBeneficiosDB _mBeneficiosDB;

        /// <summary>
        /// Inicializa el acceso a datos y la bitácora de beneficios con la configuración inyectada.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmAfBeneficiosGruposDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _mBeneficiosDB = new MBeneficiosDB(_config);
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
        /// <param name="filtro">Filtro por código, descripción o categoría.</param>
        /// <returns>Lista de grupos y total.</returns>
        public ErrorDto<AfiBeneGruposLista> AfiBeneGrupos_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var response = new AfiBeneGruposLista();

                const string sqlCount = "SELECT COUNT(*) FROM AFI_BENE_GRUPOS";
                response.Total = connection.QueryFirstOrDefault<int>(sqlCount);

                var like = string.IsNullOrWhiteSpace(filtro) ? null : $"%{filtro}%";
                var offset = pagina ?? 0;
                var fetch = paginacion ?? 10;

                const string sql = @"SELECT cod_grupo, descripcion, Cod_Categoria AS cod_categoria, monto, estado,
                                            User_Registra AS user_registra, Fecha AS fecha
                                     FROM AFI_BENE_GRUPOS
                                     WHERE (@like IS NULL OR cod_grupo LIKE @like OR descripcion LIKE @like OR Cod_Categoria LIKE @like)
                                     ORDER BY cod_grupo
                                     OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY";

                response.beneficios = connection.Query<AfiBeneGrupos>(sql, new { like, offset, fetch }).ToList();
                return response;
            });
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
        /// Exporta la lista completa de grupos de beneficios.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <returns>Lista de grupos.</returns>
        public ErrorDto<List<AfiBeneGrupos>> AfiBeneGrupoExportar(int CodCliente)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = @"SELECT cod_grupo, descripcion, Cod_Categoria AS cod_categoria, monto, estado,
                                            User_Registra AS user_registra, Fecha AS fecha
                                     FROM AFI_BENE_GRUPOS ORDER BY cod_grupo";
                return connection.Query<AfiBeneGrupos>(sql).ToList();
            });
        }
    }
}
