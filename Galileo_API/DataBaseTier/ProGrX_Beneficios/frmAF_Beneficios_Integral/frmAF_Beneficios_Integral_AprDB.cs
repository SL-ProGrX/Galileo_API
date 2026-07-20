using Galileo.Models.AF;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    /// <summary>
    /// Acceso a datos del proceso Apremiantes de Beneficios Integrales (FrmAfBeneficiosIntegralApr).
    /// Constructor y helpers compartidos. Catálogos, familia, finanzas y justificaciones en parciales.
    /// </summary>
    public partial class FrmAfBeneficiosIntegralAprDB
    {
        private readonly IConfiguration _config;
        private readonly MBeneficiosDB _mBeneficiosDB;

        /// <summary>
        /// Inicializa el acceso a datos y la dependencia de validaciones con la configuración inyectada.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmAfBeneficiosIntegralAprDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _mBeneficiosDB = new MBeneficiosDB(_config);
        }

        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Devuelve el valor <c>item</c> del drop o cadena vacía si el drop es null.
        /// </summary>
        private static string ItemOrEmpty(AfBeneficioIntegralDropsLista? drop) => drop?.item ?? string.Empty;
    }
}
