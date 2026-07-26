using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;

namespace Galileo.DataBaseTier.ProGrX_BeneficiosFosol
{
    /// <summary>
    /// Acceso a datos de la Consulta de Expedientes Fosol (frmFSL_Consulta).
    /// Catálogos de apoyo aquí; consulta principal de expedientes en el parcial .Expedientes.
    /// </summary>
    public partial class FrmFslConsultaDB
    {
        private readonly IConfiguration _config;

        /// <summary>
        /// Inicializa el acceso a datos con la configuración inyectada.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmFslConsultaDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Obtiene los planes Fosol activos.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <returns>Lista de planes.</returns>
        public ErrorDto<List<FslConsultaListas>> FslConsultaPlanes_Obtener(int CodCliente)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = @"SELECT COD_PLAN AS item, COD_PLAN + ' - ' + RTRIM(Descripcion) AS descripcion
                                     FROM FSL_PLANES WHERE activo = 1 ORDER BY descripcion";
                return connection.Query<FslConsultaListas>(sql).ToList();
            });
        }

        /// <summary>
        /// Obtiene las causas activas de un plan.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="cod_plan">Código del plan.</param>
        /// <returns>Lista de causas.</returns>
        public ErrorDto<List<FslConsultaListas>> FslConsultaCausas_Obtener(int CodCliente, string cod_plan)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = @"SELECT cod_causa AS item, RTRIM(descripcion) AS descripcion
                                     FROM FSL_PLANES_CAUSAS WHERE cod_plan = @cod_plan AND Activa = 1 ORDER BY Descripcion";
                return connection.Query<FslConsultaListas>(sql, new { cod_plan }).ToList();
            });
        }

        /// <summary>
        /// Obtiene las enfermedades activas.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <returns>Lista de enfermedades.</returns>
        public ErrorDto<List<FslConsultaListas>> FslConsultaEnfermedades_Obtener(int CodCliente)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = @"SELECT cod_enfermedad AS item, DESCRIPCION AS descripcion
                                     FROM FSL_TIPOS_ENFERMEDADES WHERE Activa = 1 ORDER BY descripcion";
                return connection.Query<FslConsultaListas>(sql).ToList();
            });
        }

        /// <summary>
        /// Obtiene los comités activos.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <returns>Lista de comités.</returns>
        public ErrorDto<List<FslConsultaListas>> FslConsutaComites_Obtener(int CodCliente)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = @"SELECT cod_Comite AS item, RTRIM(cod_Comite) + ' - ' + DESCRIPCION AS descripcion
                                     FROM FSL_COMITES WHERE ACTIVO = 1 ORDER BY COD_COMITE";
                return connection.Query<FslConsultaListas>(sql).ToList();
            });
        }

        /// <summary>
        /// Obtiene los estados de persona activos.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <returns>Lista de estados.</returns>
        public ErrorDto<List<FslConsultaListas>> FslConsutaEstadoPersonas_Obtener(int CodCliente)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = @"SELECT COD_ESTADO AS item, RTRIM(COD_ESTADO) + ' - ' + DESCRIPCION AS descripcion
                                     FROM AFI_ESTADOS_PERSONA WHERE Activo = 1 ORDER BY COD_ESTADO";
                return connection.Query<FslConsultaListas>(sql).ToList();
            });
        }

        /// <summary>
        /// Obtiene los tipos de gestión activos.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <returns>Lista de tipos de gestión.</returns>
        public ErrorDto<List<FslConsultaListas>> FslConsutaTiposGestion_Obtener(int CodCliente)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = @"SELECT COD_GESTION AS item, RTRIM(COD_GESTION) + ' - ' + DESCRIPCION AS descripcion
                                     FROM FSL_TIPOS_GESTIONES WHERE Activa = 1 ORDER BY COD_GESTION";
                return connection.Query<FslConsultaListas>(sql).ToList();
            });
        }

        /// <summary>
        /// Obtiene los tipos de apelación activos.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <returns>Lista de tipos de apelación.</returns>
        public ErrorDto<List<FslConsultaListas>> FslConsutaTiposApelaciones_Obtener(int CodCliente)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = @"SELECT COD_APELACION AS item, RTRIM(COD_APELACION) + ' - ' + DESCRIPCION AS descripcion
                                     FROM FSL_TIPOS_APELACIONES WHERE Activa = 1 ORDER BY COD_APELACION";
                return connection.Query<FslConsultaListas>(sql).ToList();
            });
        }

        /// <summary>
        /// Obtiene los miembros activos de un comité.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="cod_comite">Código del comité.</param>
        /// <returns>Lista de miembros.</returns>
        public ErrorDto<List<FslConsultaListas>> FslConsultaComiteMiembros_Obtener(int CodCliente, string cod_comite)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = @"SELECT cedula AS item, RTRIM(cedula) + ' - ' + RTRIM(Nombre) AS descripcion
                                     FROM FSL_COMITES_MIEMBROS WHERE cod_comite = @cod_comite AND Activo = 1 ORDER BY Nombre";
                return connection.Query<FslConsultaListas>(sql, new { cod_comite }).ToList();
            });
        }
    }
}
