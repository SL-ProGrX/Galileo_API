namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    /// <summary>
    /// Acceso a datos del proceso Personas de Beneficios Integrales (frmAF_Beneficios_Integral_Per).
    /// Constructor y helpers compartidos. Catálogos, persona y teléfonos en los parciales asociados.
    /// </summary>
    public partial class frmAF_Beneficios_Integral_PerDB
    {
        private readonly IConfiguration _config;

        /// <summary>
        /// Inicializa el acceso a datos con la configuración inyectada.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public frmAF_Beneficios_Integral_PerDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Normaliza un valor de cédula dejando solo caracteres seguros (letras, dígitos y guion),
        /// como mitigación de inyección cuando el valor se usa en consultas dinámicas (Checkmarx).
        /// </summary>
        private static string NormalizarCedula(string? cedula)
            => new string((cedula ?? string.Empty).Where(c => char.IsLetterOrDigit(c) || c == '-').ToArray());
    }
}
