using Galileo.DataBaseTier;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    /// <summary>
    /// Acceso a datos del Pago de Beneficios (frmAF_BeneficioPago).
    /// Constructor y dependencias compartidas. Consultas en .Consultas, generación de pago en .Guardar.
    /// </summary>
    public partial class FrmAfBeneficioPagoDB
    {
        private readonly IConfiguration _config;
        private readonly MTesFuncionesDb _mTes;

        /// <summary>
        /// Inicializa el acceso a datos y las funciones de tesorería con la configuración inyectada.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmAfBeneficioPagoDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _mTes = new MTesFuncionesDb(_config);
        }

        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        private PortalDB CreatePortalDb() => new(_config);
    }
}
