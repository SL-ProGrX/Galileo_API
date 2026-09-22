namespace Galileo.DataBaseTier
{
    public sealed partial class FrmInvTomaFisicaDB
    {
        private const int CodigoValidacion = -2;
        private const int ModuloInventarios = 32;
        private const int PaginacionPredeterminada = 30;
        private const int PaginacionMaxima = 100;
        private const int LongitudFiltroMaxima = 150;
        private const int TiempoEsperaInventario = 120;

        private const string EstadoSolicitado = "S";
        private const string TipoCodigoBarras = "CB";
        private const string TipoCodigoProducto = "CP";
        private const string DireccionAscendente = "asc";
        private const string DireccionDescendente = "desc";

        private const string MensajeTomaRequerida =
            "La informaci&oacute;n de la toma f&iacute;sica es requerida.";
        private const string MensajeConsecutivoRequerido =
            "El consecutivo de la toma f&iacute;sica es requerido.";
        private const string MensajeBodegaRequerida =
            "El c&oacute;digo de la bodega es requerido.";
        private const string MensajeBodegaNoExiste =
            "La bodega indicada no existe.";
        private const string MensajeFechaInicioRequerida =
            "La fecha inicial es requerida.";
        private const string MensajeFechaCorteRequerida =
            "La fecha de corte es requerida.";
        private const string MensajeUsuarioRequerido =
            "El usuario es requerido.";
        private const string MensajeProductoRequerido =
            "El c&oacute;digo del producto es requerido.";
        private const string MensajeProductoNoExiste =
            "Uno o m&aacute;s productos no existen.";
        private const string MensajeTomaNoExiste =
            "La toma f&iacute;sica indicada no existe.";
        private const string MensajeTomaProcesada =
            "No se puede modificar la toma f&iacute;sica porque ya fue procesada.";
        private const string MensajeProductoDuplicado =
            "El producto ya se encuentra registrado en la misma ubicaci&oacute;n.";

        private const string ErrorConsultarLista =
            "Ocurri&oacute; un error al consultar las tomas f&iacute;sicas.";
        private const string ErrorConsultarDetalle =
            "Ocurri&oacute; un error al consultar el detalle de la toma f&iacute;sica.";
        private const string ErrorConsultarToma =
            "Ocurri&oacute; un error al consultar la toma f&iacute;sica.";
        private const string ErrorConsultarProducto =
            "Ocurri&oacute; un error al consultar el producto.";
        private const string ErrorGuardar =
            "Ocurri&oacute; un error al guardar la toma f&iacute;sica.";
        private const string ErrorEliminar =
            "Ocurri&oacute; un error al eliminar la toma f&iacute;sica.";
        private const string ErrorInventarioLogico =
            "Ocurri&oacute; un error al obtener el inventario l&oacute;gico.";
        private const string ErrorComparar =
            "Ocurri&oacute; un error al comparar la toma f&iacute;sica.";
        private const string ErrorGuardarBarras =
            "Ocurri&oacute; un error al guardar el producto capturado.";

        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _securityMainDb;
        public FrmInvTomaFisicaDB(IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _portalDb = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }
    }
}
