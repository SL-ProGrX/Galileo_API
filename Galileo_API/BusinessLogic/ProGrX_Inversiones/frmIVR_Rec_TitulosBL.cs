using Galileo.DataBaseTier.ProGrX_Inversiones;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Inversiones;

namespace Galileo_API.BusinessLogic.ProGrX_Inversiones
{
    public sealed class FrmIvrRecTitulosBl
    {
        private readonly FrmIvrRecTitulosDb _db;

        public FrmIvrRecTitulosBl(IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            _db = new FrmIvrRecTitulosDb(config);
        }

        public ErrorDto<IvrRecTitulosInicialData> IVR_Rec_Titulos_Inicializar(int codEmpresa) =>
            _db.IVR_Rec_Titulos_Inicializar(codEmpresa);

        public ErrorDto<List<IvrRecTitulosBusquedaData>> IVR_Rec_Titulos_Buscar(
            int codEmpresa,
            string? filtro) =>
            _db.IVR_Rec_Titulos_Buscar(codEmpresa, filtro);

        public ErrorDto<IvrRecTitulosNavegacionData?> IVR_Rec_Titulos_Navegar(
            int codEmpresa,
            long tituloId,
            bool siguiente) =>
            _db.IVR_Rec_Titulos_Navegar(codEmpresa, tituloId, siguiente);

        public ErrorDto<IvrRecTitulosData?> IVR_Rec_Titulos_Obtener(
            int codEmpresa,
            long tituloId) =>
            _db.IVR_Rec_Titulos_Obtener(codEmpresa, tituloId);

        public ErrorDto<List<IvrRecTitulosTransaccionData>> IVR_Rec_Titulos_Adquisiciones_Obtener(
            int codEmpresa,
            long tituloId) =>
            _db.IVR_Rec_Titulos_Adquisiciones_Obtener(codEmpresa, tituloId);

        public ErrorDto<List<IvrRecTitulosComisionData>> IVR_Rec_Titulos_Comisiones_Obtener(
            int codEmpresa,
            long tituloId) =>
            _db.IVR_Rec_Titulos_Comisiones_Obtener(codEmpresa, tituloId);

        public ErrorDto<List<IvrRecTitulosFlujoData>> IVR_Rec_Titulos_Flujos_Obtener(
            int codEmpresa,
            long tituloId,
            string? tipo) =>
            _db.IVR_Rec_Titulos_Flujos_Obtener(codEmpresa, tituloId, tipo);

        public ErrorDto<List<IvrRecTitulosCierreData>> IVR_Rec_Titulos_Cierres_Obtener(
            int codEmpresa,
            long tituloId) =>
            _db.IVR_Rec_Titulos_Cierres_Obtener(codEmpresa, tituloId);

        public ErrorDto<List<IvrRecTitulosFondoData>> IVR_Rec_Titulos_Fondos_Obtener(
            int codEmpresa,
            long tituloId,
            string? estado,
            string? tipo) =>
            _db.IVR_Rec_Titulos_Fondos_Obtener(codEmpresa, tituloId, estado, tipo);

        public ErrorDto<List<IvrRecTitulosCuponData>> IVR_Rec_Titulos_Cupones_Obtener(
            int codEmpresa,
            long tituloId,
            bool proyectados) =>
            _db.IVR_Rec_Titulos_Cupones_Obtener(codEmpresa, tituloId, proyectados);

        public ErrorDto<List<IvrRecTitulosAsientoData>> IVR_Rec_Titulos_Asientos_Obtener(
            int codEmpresa,
            long tituloId) =>
            _db.IVR_Rec_Titulos_Asientos_Obtener(codEmpresa, tituloId);

        public ErrorDto<List<IvrRecTitulosAsientoDetalleData>> IVR_Rec_Titulos_Asiento_Titulo_Obtener(
            int codEmpresa,
            long tituloId) =>
            _db.IVR_Rec_Titulos_Asiento_Titulo_Obtener(codEmpresa, tituloId);

        public ErrorDto<List<IvrRecTitulosAsientoDetalleData>> IVR_Rec_Titulos_Asiento_Detalle_Obtener(
            int codEmpresa,
            string? tipoAsiento,
            string? numeroAsiento) =>
            _db.IVR_Rec_Titulos_Asiento_Detalle_Obtener(codEmpresa, tipoAsiento, numeroAsiento);

        public ErrorDto<IvrRecTitulosGuardarData> IVR_Rec_Titulos_Registrar(
            int codEmpresa,
            IvrRecTitulosGuardarRequest? request) =>
            _db.IVR_Rec_Titulos_Registrar(codEmpresa, request);

        public ErrorDto<IvrRecTitulosGuardarData> IVR_Rec_Titulos_Actualizar(
            int codEmpresa,
            IvrRecTitulosGuardarRequest? request) =>
            _db.IVR_Rec_Titulos_Actualizar(codEmpresa, request);

        public ErrorDto<IvrRecTitulosGuardarData> IVR_Rec_Titulos_Activar(
            int codEmpresa,
            IvrRecTitulosAccionRequest? request) =>
            _db.IVR_Rec_Titulos_Activar(codEmpresa, request);

        public ErrorDto IVR_Rec_Titulos_Adquisicion_Eliminar(
            int codEmpresa,
            IvrRecTitulosTransaccionEliminarRequest? request) =>
            _db.IVR_Rec_Titulos_Adquisicion_Eliminar(codEmpresa, request);

        public ErrorDto IVR_Rec_Titulos_Pyd_Recalcular(
            int codEmpresa,
            IvrRecTitulosAccionRequest? request) =>
            _db.IVR_Rec_Titulos_Pyd_Recalcular(codEmpresa, request);
    }
}
