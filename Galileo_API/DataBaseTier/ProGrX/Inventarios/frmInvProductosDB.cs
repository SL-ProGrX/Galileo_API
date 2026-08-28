namespace Galileo.DataBaseTier
{
    /// <summary>
    /// Coordina el acceso a datos del mantenimiento de productos de inventario.
    /// Las operaciones específicas se mantienen en archivos parciales trazables.
    /// </summary>
    public partial class FrmInvProductosDB
    {
        private readonly IConfiguration _config;
        private readonly MProGrXAuxiliarDB mAuxiliarDB;

        /// <summary>
        /// Inicializa el acceso a datos de productos.
        /// </summary>
        /// <param name="config">Configuración y cadenas de conexión de la aplicación.</param>
        public FrmInvProductosDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            mAuxiliarDB = new MProGrXAuxiliarDB(_config);
        }
    }
}
