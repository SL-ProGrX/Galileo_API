using Galileo.Models;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    /// <summary>
    /// Acceso a datos del formulario principal de Beneficios Integrales (frmAF_Beneficios_Integral).
    /// Constructor y dependencias compartidas. Catálogos, observaciones, consultas y procesos en parciales.
    /// </summary>
    public partial class frmAF_Beneficios_IntegralDB
    {
        private readonly IConfiguration _config;
        private readonly MBeneficiosDB _mBeneficiosDB;
        private readonly EnvioCorreoDB _envioCorreoDB;
        private readonly string _sendEmail;
        private readonly string _notificacionCobros;

        /// <summary>
        /// Inicializa el acceso a datos y las dependencias (bitácora, correo) con la configuración inyectada.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public frmAF_Beneficios_IntegralDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _mBeneficiosDB = new MBeneficiosDB(_config);
            _envioCorreoDB = new EnvioCorreoDB(_config);
            _sendEmail = _config.GetSection("AppSettings").GetSection("EnviaEmail").Value ?? "N";
            _notificacionCobros = _config.GetSection("AFI_Beneficios").GetSection("NotificacionCobros").Value ?? string.Empty;
        }

        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        private PortalDB CreatePortalDb() => new(_config);

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
