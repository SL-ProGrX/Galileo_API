using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;

namespace Galileo.DataBaseTier.ProGrX_BeneficiosFosol
{
    /// <summary>
    /// Acceso a datos de los Reportes de Beneficios Fosol (frmFSL_Reportes).
    /// </summary>
    public partial class FrmFslReportesDB
    {
        private readonly IConfiguration _config;

        /// <summary>
        /// Inicializa el acceso a datos con la configuración inyectada.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmFslReportesDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Obtiene el catálogo de oficinas.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <returns>Lista de oficinas.</returns>
        public ErrorDto<List<Oficina>> FSL_Oficinas_Obtener(int CodEmpresa)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                const string sql = @"SELECT RTRIM(cod_oficina) AS item, RTRIM(descripcion) AS descripcion
                                     FROM SIF_Oficinas ORDER BY cod_oficina";
                return connection.Query<Oficina>(sql).ToList();
            });

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse<List<Oficina>>("FSL_Oficinas_Obtener - " + result.Description);
            }

            return result;
        }

        /// <summary>
        /// Obtiene el catálogo de planes Fosol activos.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <returns>Lista de planes.</returns>
        public ErrorDto<List<Plan>> FSL_Planes_Obtener(int CodEmpresa)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                const string sql = @"SELECT RTRIM(COD_PLAN) AS item, RTRIM(COD_PLAN) + ' - ' + descripcion AS descripcion
                                     FROM FSL_PLANES WHERE ACTIVO = 1 ORDER BY cod_plan";
                return connection.Query<Plan>(sql).ToList();
            });

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse<List<Plan>>("FSL_Planes_Obtener - " + result.Description);
            }

            return result;
        }
    }
}
