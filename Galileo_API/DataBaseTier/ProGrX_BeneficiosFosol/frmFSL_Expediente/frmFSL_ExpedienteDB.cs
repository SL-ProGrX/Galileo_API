using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;

namespace Galileo.DataBaseTier.ProGrX_BeneficiosFosol
{
    /// <summary>
    /// Acceso a datos de los Expedientes Fosol (frmFSL_Expediente).
    /// Catálogos aquí; consultas en .Consultas, guardado en .Guardar, validaciones en .Validaciones.
    /// </summary>
    public partial class FrmFslExpedienteDB
    {
        private readonly IConfiguration _config;

        /// <summary>
        /// Inicializa el acceso a datos con la configuración inyectada.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmFslExpedienteDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Obtiene la lista de planes activos.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <returns>Lista de planes.</returns>
        public ErrorDto<List<FslMenusData>> FslPlanLista_Obtener(int CodCliente)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = @"SELECT COD_PLAN AS item, RTRIM(COD_PLAN) + ' - ' + RTRIM(descripcion) AS descripcion
                                     FROM FSL_PLANES WHERE activo = 1";
                return connection.Query<FslMenusData>(sql).ToList();
            });
        }

        /// <summary>
        /// Obtiene la lista de comités activos.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <returns>Lista de comités.</returns>
        public ErrorDto<List<FslMenusData>> FslComiteLista_Obtener(int CodCliente)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = @"SELECT COD_COMITE AS item, RTRIM(COD_COMITE) + ' - ' + RTRIM(descripcion) AS descripcion
                                     FROM FSL_COMITES WHERE activo = 1";
                return connection.Query<FslMenusData>(sql).ToList();
            });
        }

        /// <summary>
        /// Obtiene la lista de enfermedades activas.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <returns>Lista de enfermedades.</returns>
        public ErrorDto<List<FslMenusData>> FslEnfermedadesLista_Obtener(int CodCliente)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = @"SELECT COD_ENFERMEDAD AS item, RTRIM(COD_ENFERMEDAD) + ' - ' + RTRIM(descripcion) AS descripcion
                                     FROM FSL_TIPOS_ENFERMEDADES WHERE ACTIVA = 1";
                return connection.Query<FslMenusData>(sql).ToList();
            });
        }

        /// <summary>
        /// Obtiene la lista de causas de un plan.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="cod_plan">Código del plan.</param>
        /// <returns>Lista de causas.</returns>
        public ErrorDto<List<FslMenusData>> FslCausasLista_Obtener(int CodCliente, string cod_plan)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = @"SELECT COD_CAUSA AS item, RTRIM(COD_CAUSA) + ' - ' + RTRIM(descripcion) AS descripcion
                                     FROM FSL_PLANES_CAUSAS WHERE COD_PLAN = @cod_plan ORDER BY COD_CAUSA";
                return connection.Query<FslMenusData>(sql, new { cod_plan }).ToList();
            });
        }
    }
}
