using Galileo.Models.AF;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    /// <summary>
    /// Acceso a datos del proceso Reconocimientos de Beneficios Integrales (FrmAfBeneficiosIntegralRec).
    /// Constructor y helpers compartidos. Consultas, persistencia y validaciones en los parciales asociados.
    /// </summary>
    public partial class FrmAfBeneficiosIntegralRecDB
    {
        private readonly IConfiguration _config;

        /// <summary>
        /// Inicializa el acceso a datos con la configuración inyectada.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmAfBeneficiosIntegralRecDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
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
