using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using Galileo.Models.Security;
using Newtonsoft.Json;

namespace Galileo.DataBaseTier.ProGrX_BeneficiosFosol
{
    /// <summary>
    /// Acceso a datos de los Tipos de Aplicación Fosol (frmFSL_TipoAplicacion): planes y causas.
    /// Consultas aquí; guardado en el parcial .Guardar.
    /// </summary>
    public partial class FrmFslTipoAplicacionDB
    {
        private const string SqlCausasSelect = @"
            SELECT COD_CAUSA AS cod_causa, descripcion,
                   CASE WHEN MONTO_BASE = 'F' THEN 'Formalizado' ELSE 'Saldo' END AS montoBase,
                   CASE WHEN TIPO_TABLA = 'F' THEN 'Fallecimiento' WHEN TIPO_TABLA = 'I' THEN 'Incapacidad'
                        WHEN TIPO_TABLA = 'X' THEN '100 %' WHEN TIPO_TABLA = 'S' THEN 'Suicidio' ELSE 'Fallecimiento' END AS tipoTabla,
                   Activa AS activa
            FROM FSL_PLANES_CAUSAS";

        private const string SqlPlanesSelect = @"
            SELECT COD_PLAN AS cod_plan, descripcion,
                   CASE WHEN ISNULL(Tipo_Desembolso, 'F') = 'F' THEN 'Fondos' ELSE 'Tesorería' END AS tipo,
                   Activo AS activo
            FROM FSL_PLANES";

        private readonly IConfiguration _config;

        /// <summary>
        /// Inicializa el acceso a datos con la configuración inyectada.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmFslTipoAplicacionDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Obtiene las causas de un plan con paginación y filtro.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="TipoCausa">Código del plan.</param>
        /// <param name="Jfiltro">JSON con filtro y paginación.</param>
        /// <returns>Lista de causas y total.</returns>
        public ErrorDto<CausasDataLista> Causas_Obtener(int CodCliente, string TipoCausa, string Jfiltro)
        {
            var filtro = JsonConvert.DeserializeObject<FiltroLazy>(Jfiltro) ?? new FiltroLazy();

            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var response = new CausasDataLista();

                const string sqlCount = "SELECT COUNT(*) FROM FSL_PLANES_CAUSAS WHERE COD_PLAN = @TipoCausa";
                response.total = connection.QueryFirstOrDefault<int>(sqlCount, new { TipoCausa });

                var like = string.IsNullOrWhiteSpace(filtro.filtro) ? null : $"%{filtro.filtro}%";
                var offset = filtro.pagina ?? 0;
                var fetch = filtro.paginacion ?? 10;

                var sql = $@"{SqlCausasSelect}
                             WHERE COD_PLAN = @TipoCausa
                               AND (@like IS NULL OR COD_CAUSA LIKE @like OR descripcion LIKE @like)
                             ORDER BY COD_CAUSA
                             OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY";

                response.causas = connection.Query<TiposCausaData>(sql, new { TipoCausa, like, offset, fetch }).ToList();
                return response;
            });
        }

        /// <summary>
        /// Exporta la lista completa de causas de un plan.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="TipoCausa">Código del plan.</param>
        /// <returns>Lista de causas.</returns>
        public ErrorDto<List<TiposCausaData>> CausasListas_Exportar(int CodCliente, string TipoCausa)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var sql = $"{SqlCausasSelect} WHERE COD_PLAN = @TipoCausa ORDER BY COD_CAUSA";
                return connection.Query<TiposCausaData>(sql, new { TipoCausa }).ToList();
            });
        }

        /// <summary>
        /// Obtiene los planes Fosol con paginación y filtro.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="Jfiltro">JSON con filtro y paginación.</param>
        /// <returns>Lista de planes y total.</returns>
        public ErrorDto<PlanesDataLista> Planes_Obtener(int CodCliente, string Jfiltro)
        {
            var filtro = JsonConvert.DeserializeObject<FiltroLazy>(Jfiltro) ?? new FiltroLazy();

            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var response = new PlanesDataLista();

                const string sqlCount = "SELECT COUNT(*) FROM FSL_PLANES";
                response.total = connection.QueryFirstOrDefault<int>(sqlCount);

                var like = string.IsNullOrWhiteSpace(filtro.filtro) ? null : $"%{filtro.filtro}%";
                var offset = filtro.pagina ?? 0;
                var fetch = filtro.paginacion ?? 10;

                var sql = $@"{SqlPlanesSelect}
                             WHERE (@like IS NULL OR COD_PLAN LIKE @like OR DESCRIPCION LIKE @like)
                             ORDER BY COD_PLAN
                             OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY";

                response.planes = connection.Query<ListaPlanesData>(sql, new { like, offset, fetch }).ToList();
                return response;
            });
        }

        /// <summary>
        /// Exporta la lista completa de planes.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <returns>Lista de planes.</returns>
        public ErrorDto<List<ListaPlanesData>> PlanesLista_Exportar(int CodCliente)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var sql = $"{SqlPlanesSelect} ORDER BY COD_PLAN";
                return connection.Query<ListaPlanesData>(sql).ToList();
            });
        }

        /// <summary>
        /// Obtiene la lista simple de planes activos.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <returns>Lista de planes.</returns>
        public ErrorDto<List<ListaPlanesData>> ListaPlanes_Obtener(int CodCliente)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = @"SELECT RTRIM(COD_PLAN) AS cod_plan, RTRIM(COD_PLAN) + ' - ' + descripcion AS descripcion
                                     FROM FSL_PLANES WHERE ACTIVO = 1";
                return connection.Query<ListaPlanesData>(sql).ToList();
            });
        }
    }
}
