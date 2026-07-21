using Galileo.Models;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    /// <summary>
    /// Acceso a datos del proceso Generales de Beneficios Integrales (FrmAfBeneficiosIntegralGen).
    /// Constructor y helpers compartidos. Catálogos, consultas, validaciones, mora, notificaciones y pendientes en parciales.
    /// </summary>
    public partial class FrmAfBeneficiosIntegralGenDB
    {
        private readonly IConfiguration _config;
        private readonly MBeneficiosDB _mBeneficiosDB;
        private readonly EnvioCorreoDB _envioCorreoDB;
        private readonly FrmAfBeneficioAsgDB _frmAsgDb;
        private readonly string _sendEmail;
        private readonly string _notificacionCobros;

        /** Indica si el beneficio aplica pago parcial (usado por el flujo de guardado).
        private bool _bAplicaParcial;
        **/

        /// <summary>
        /// Inicializa el acceso a datos y las dependencias (validaciones, bitácora, correo, asignación) con la configuración inyectada.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmAfBeneficiosIntegralGenDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _mBeneficiosDB = new MBeneficiosDB(_config);
            _envioCorreoDB = new EnvioCorreoDB(_config);
            _frmAsgDb = new FrmAfBeneficioAsgDB(_config);
            _sendEmail = _config.GetSection("AppSettings").GetSection("EnviaEmail").Value ?? "N";
            _notificacionCobros = _config.GetSection("AFI_Beneficios").GetSection("NotificacionCobros").Value ?? string.Empty;
        }

        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Normaliza un texto dejando solo caracteres seguros (letras, dígitos y guion),
        /// como mitigación de inyección cuando el valor se usa en consultas dinámicas (Checkmarx).
        /// </summary>
        private static string NormalizarSeguro(string? valor)
            => new string((valor ?? string.Empty).Where(c => char.IsLetterOrDigit(c) || c == '-' || c == '_').ToArray());

        /// <summary>
        /// Registra un movimiento en la bitácora de beneficios (helper compartido).
        /// </summary>
        private void RegistrarBitacora(int CodCliente, string codBeneficio, long? consec, string movimiento, string detalle, string usuario)
        {
            _mBeneficiosDB.BitacoraBeneficios(new BitacoraBeneInsertarDto
            {
                EmpresaId = CodCliente,
                cod_beneficio = codBeneficio,
                consec = consec,
                movimiento = movimiento,
                detalle = detalle,
                registro_usuario = usuario
            });
        }
    }
}
