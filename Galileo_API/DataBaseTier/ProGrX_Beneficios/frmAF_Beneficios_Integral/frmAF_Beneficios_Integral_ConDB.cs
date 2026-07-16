using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    /// <summary>
    /// Acceso a datos del proceso Consultas de Beneficios Integrales (frmAF_Beneficios_Integral_Con).
    /// Este parcial contiene el constructor y los helpers compartidos.
    /// Las consultas viven en el parcial frmAF_Beneficios_Integral_ConDB.Consultas.cs.
    /// </summary>
    public partial class frmAF_Beneficios_Integral_ConDB
    {
        private readonly IConfiguration _config;

        /// <summary>
        /// Inicializa el acceso a datos con la configuración inyectada.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public frmAF_Beneficios_Integral_ConDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        /// <returns>Instancia de PortalDB configurada.</returns>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Normaliza texto de filtros: evita null y recorta espacios externos.
        /// </summary>
        /// <param name="valor">Valor original.</param>
        /// <returns>Texto sin espacios externos o cadena vacía.</returns>
        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();
    }
}
