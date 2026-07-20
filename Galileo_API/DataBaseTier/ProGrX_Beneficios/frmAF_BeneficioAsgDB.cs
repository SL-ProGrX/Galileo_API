using Galileo.Models;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    /// <summary>
    /// Acceso a datos del formulario de Asignación de Beneficios (frmAF_BeneficioAsg).
    /// Constructor y helpers compartidos. Consultas, cuentas, montos y guardado en parciales.
    /// </summary>
    public partial class FrmAfBeneficioAsgDB
    {
        private readonly IConfiguration _config;
        private readonly MSecurityMainDb _bitacoraDb;
        private readonly MProGrxMain _mProGrxMain;

        // Estado compartido entre fxMonto y fxValida (instancia por request).
        private FxMontoModel _datosBase = new FxMontoModel();
        private bool _bAplicaParcial;

        /// <summary>
        /// Inicializa el acceso a datos y las dependencias (bitácora, tags) con la configuración inyectada.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmAfBeneficioAsgDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _bitacoraDb = new MSecurityMainDb(_config);
            _mProGrxMain = new MProGrxMain(_config);
        }

        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>Registra un movimiento en la bitácora de seguridad.</summary>
        public ErrorDto Bitacora(BitacoraInsertarDto data) => _bitacoraDb.Bitacora(data);

        /// <summary>Registra los tags SIF del proceso.</summary>
        public ErrorDto SbSIFRegistraTags(SifRegistraTagsRequestDto data) => _mProGrxMain.SbSIFRegistraTags(data);
    }
}
