using Dapper;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    /// <summary>
    /// Acceso a datos del formulario de Reportes de Beneficios (frmAF_BeneficioReporte).
    /// </summary>
    public partial class FrmAfBeneficioReporteDB
    {
        private readonly IConfiguration _config;

        /// <summary>
        /// Inicializa el acceso a datos con la configuración inyectada.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmAfBeneficioReporteDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Obtiene la lista de beneficios (código y descripción) para reportes.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <returns>Lista de beneficios.</returns>
        public ErrorDto<List<AfiBeneficiosData>> BeneficioLista_Obtener(int CodCliente)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = @"SELECT RTRIM(cod_Beneficio) AS item, RTRIM(descripcion) AS descripcion
                                     FROM afi_beneficios";
                return connection.Query<AfiBeneficiosData>(sql).ToList();
            });

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse<List<AfiBeneficiosData>>("BeneficioLista_Obtener - " + result.Description);
            }

            return result;
        }
    }
}
