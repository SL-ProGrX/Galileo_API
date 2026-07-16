namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    /// <summary>
    /// Acceso a datos del proceso Crece de Beneficios Integrales (frmAF_Beneficios_Integral_Cre).
    /// Constructor y dependencias compartidas. Consultas y persistencia en los parciales asociados.
    /// </summary>
    public partial class frmAF_Beneficios_Integral_CreDB
    {
        private readonly IConfiguration _config;
        private readonly MBeneficiosDB _mBeneficiosDB;

        /// <summary>
        /// Inicializa el acceso a datos y la dependencia de bitácora con la configuración inyectada.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public frmAF_Beneficios_Integral_CreDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _mBeneficiosDB = new MBeneficiosDB(_config);
        }

        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        private PortalDB CreatePortalDb() => new(_config);
    }
}
