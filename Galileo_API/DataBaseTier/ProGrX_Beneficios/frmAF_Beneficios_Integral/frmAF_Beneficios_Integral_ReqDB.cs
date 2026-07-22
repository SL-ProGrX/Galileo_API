namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    /// <summary>
    /// Acceso a datos del proceso Requisitos de Beneficios Integrales (FrmAfBeneficiosIntegralReq).
    /// Constructor y dependencias compartidas. Consultas y persistencia en los parciales asociados.
    /// </summary>
    public partial class FrmAfBeneficiosIntegralReqDB
    {
        private readonly IConfiguration _config;
        private readonly MBeneficiosDB _mBeneficiosDB;

        /// <summary>
        /// Inicializa el acceso a datos y la dependencia de bitácora con la configuración inyectada.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmAfBeneficiosIntegralReqDB(IConfiguration config)
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
