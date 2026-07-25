using Dapper;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    /// <summary>
    /// Acceso a datos del Monitor de Beneficios (frmAF_Beneficios_Monitor).
    /// Catálogos de apoyo aquí; consulta principal y filtros en el parcial .Consultas.
    /// </summary>
    public partial class FrmAfBeneficiosMonitorDB
    {
        private readonly IConfiguration _config;

        /// <summary>
        /// Inicializa el acceso a datos con la configuración inyectada.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmAfBeneficiosMonitorDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Obtiene la lista de instituciones.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <returns>Lista de instituciones.</returns>
        public ErrorDto<List<OpcionesLista>> InstitucionesLista_Obtener(int CodCliente)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = @"SELECT RTRIM(descripcion) AS descripcion, cod_institucion AS item
                                     FROM instituciones ORDER BY descripcion";
                return connection.Query<OpcionesLista>(sql).ToList();
            });
        }

        /// <summary>
        /// Obtiene la lista de estados de persona.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <returns>Lista de estados.</returns>
        public ErrorDto<List<OpcionesLista>> EstadosLista_Obtener(int CodCliente)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = @"SELECT RTRIM(cod_estado) AS item, RTRIM(descripcion) AS descripcion
                                     FROM afi_Estados_Persona";
                return connection.Query<OpcionesLista>(sql).ToList();
            });
        }

        /// <summary>
        /// Obtiene la lista de oficinas.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <returns>Lista de oficinas.</returns>
        public ErrorDto<List<OpcionesLista>> OficinasLista_Obtener(int CodCliente)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = @"SELECT RTRIM(cod_Oficina) AS item, RTRIM(descripcion) AS descripcion
                                     FROM SIF_Oficinas ORDER BY Descripcion";
                return connection.Query<OpcionesLista>(sql).ToList();
            });
        }

        /// <summary>
        /// Obtiene la lista de beneficios activos.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <returns>Lista de beneficios.</returns>
        public ErrorDto<List<OpcionesLista>> BeneficiosLista_Obtener(int CodCliente)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = @"SELECT COD_BENEFICIO AS item, RTRIM(DESCRIPCION) AS descripcion
                                     FROM AFI_BENEFICIOS WHERE Estado = 'A' ORDER BY descripcion";
                return connection.Query<OpcionesLista>(sql).ToList();
            });
        }
    }
}
