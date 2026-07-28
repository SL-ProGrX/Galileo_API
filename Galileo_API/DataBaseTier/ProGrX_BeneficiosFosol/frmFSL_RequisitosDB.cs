using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using Newtonsoft.Json;

namespace Galileo.DataBaseTier.ProGrX_BeneficiosFosol
{
    /// <summary>
    /// Acceso a datos de los Requisitos Fosol (frmFSL_Requisitos).
    /// Consultas aquí; guardado y asignaciones en el parcial .Guardar.
    /// </summary>
    public partial class FrmFslRequisitosDB
    {
        private readonly IConfiguration _config;

        /// <summary>
        /// Inicializa el acceso a datos con la configuración inyectada.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmFslRequisitosDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Obtiene la lista de requisitos Fosol con paginación y filtro.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="filtros">JSON con filtro de búsqueda y paginación.</param>
        /// <returns>Lista de requisitos y total.</returns>
        public ErrorDto<FslRequisitosDataLista> FslRequisitos_Obtener(int CodCliente, string filtros)
        {
            var filtro = JsonConvert.DeserializeObject<FslRequisitosFiltros>(filtros) ?? new FslRequisitosFiltros();

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var response = new FslRequisitosDataLista();

                // Nota: el original contaba sobre FSL_COMITES (bug); se corrige a FSL_REQUISITOS para un total coherente.
                const string sqlCount = "SELECT COUNT(*) FROM FSL_REQUISITOS";
                response.Total = connection.QueryFirstOrDefault<int>(sqlCount);

                var like = string.IsNullOrWhiteSpace(filtro.filtro) ? null : $"%{filtro.filtro}%";
                var offset = filtro.pagina ?? 0;
                var fetch = filtro.paginacion ?? 10;

                const string sql = @"SELECT COD_REQUISITO AS cod_requisito, DESCRIPCION AS descripcion, ACTIVO AS activo,
                                            REGISTRO_USUARIO AS registro_usuario
                                     FROM FSL_REQUISITOS
                                     WHERE (@like IS NULL OR COD_REQUISITO LIKE @like OR DESCRIPCION LIKE @like)
                                     ORDER BY COD_REQUISITO
                                     OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY";

                response.requisitos = connection.Query<FslRequisitosData>(sql, new { like, offset, fetch }).ToList();
                return response;
            });

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse<FslRequisitosDataLista>("FslRequisitos_Obtener - " + result.Description);
            }

            return result;
        }

        /// <summary>
        /// Obtiene las causas activas de un plan.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="cod_plan">Código del plan.</param>
        /// <returns>Lista de causas.</returns>
        public ErrorDto<List<FslPanesCausasLista>> FslPlanesCausa_Obtener(int CodCliente, string cod_plan)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = @"SELECT RTRIM(cod_Causa) AS item, RTRIM(descripcion) AS descripcion
                                     FROM FSL_Planes_Causas WHERE activa = 1 AND cod_plan = @cod_plan";
                return connection.Query<FslPanesCausasLista>(sql, new { cod_plan }).ToList();
            });

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse<List<FslPanesCausasLista>>("FslPlanesCausa_Obtener - " + result.Description);
            }

            return result;
        }

        /// <summary>
        /// Obtiene los requisitos y su asignación a una causa/plan.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="cod_plan">Código del plan.</param>
        /// <param name="cod_causa">Código de la causa.</param>
        /// <returns>Lista de requisitos con su asignación.</returns>
        public ErrorDto<List<FslRequisitoCausa>> FslRequisitoCausa_Obtener(int CodCliente, string cod_plan, string cod_causa)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = @"SELECT Rq.COD_REQUISITO AS cod_requisito, Rq.DESCRIPCION AS descripcion,
                                            ISNULL(Rc.OPCIONAL, 0) AS opcional, RC.COD_CAUSA AS cod_causa, RC.COD_PLAN AS cod_plan,
                                            ISNULL(Rc.ASIGNADO, 0) AS asignado
                                     FROM FSL_REQUISITOS Rq
                                     LEFT JOIN FSL_REQUISITOS_CAUSAS Rc ON Rq.COD_REQUISITO = Rc.COD_REQUISITO
                                     WHERE Rq.ACTIVO = 1 AND RC.COD_CAUSA = @cod_causa AND Rc.COD_PLAN = @cod_plan";
                return connection.Query<FslRequisitoCausa>(sql, new { cod_causa, cod_plan }).ToList();
            });

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse<List<FslRequisitoCausa>>("FslRequisitoCausa_Obtener - " + result.Description);
            }

            return result;
        }

        /// <summary>
        /// Obtiene los planes Fosol activos.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <returns>Lista de planes.</returns>
        public ErrorDto<List<FslPlanes>> FslPlanes_Obtener(int CodCliente)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = "SELECT RTRIM(cod_Plan) AS item, RTRIM(descripcion) AS descripcion FROM FSL_Planes WHERE activo = 1";
                return connection.Query<FslPlanes>(sql).ToList();
            });

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse<List<FslPlanes>>("FslPlanes_Obtener - " + result.Description);
            }

            return result;
        }
    }
}
