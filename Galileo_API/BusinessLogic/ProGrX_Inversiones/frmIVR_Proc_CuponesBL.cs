using Galileo.DataBaseTier.ProGrX_Inversiones;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Inversiones;

namespace Galileo_API.BusinessLogic.ProGrX_Inversiones
{
    public sealed class FrmIvrProcCuponesBl
    {
        private readonly FrmIvrProcCuponesDb _db;

        public FrmIvrProcCuponesBl(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _db = new FrmIvrProcCuponesDb(config);
        }

        public ErrorDto<IvrProcCuponesInicialData>
            IVR_Proc_Cupones_Inicializar(
                int codEmpresa,
                IvrProcCuponesInicializarRequest? request)
        {
            return _db.IVR_Proc_Cupones_Inicializar(
                codEmpresa,
                request);
        }

        public ErrorDto<IvrProcCuponesTransaccionesData>
            IVR_Proc_Cupones_Transacciones_Obtener(
                int codEmpresa,
                int transacId)
        {
            return _db
                .IVR_Proc_Cupones_Transacciones_Obtener(
                    codEmpresa,
                    transacId);
        }

        public ErrorDto<IvrProcCuponesGuardarData>
            IVR_Proc_Cupones_Solicitud_Guardar(
                int codEmpresa,
                IvrProcCuponesGuardarRequest? request)
        {
            return _db
                .IVR_Proc_Cupones_Solicitud_Guardar(
                    codEmpresa,
                    request);
        }

        public ErrorDto<IvrProcCuponesGuardarData>
            IVR_Proc_Cupones_Procesar(
                int codEmpresa,
                IvrProcCuponesGuardarRequest? request)
        {
            return _db.IVR_Proc_Cupones_Procesar(
                codEmpresa,
                request);
        }

        public ErrorDto<IvrProcCuponesGuardarData>
            IVR_Proc_Cupones_Eliminar(
                int codEmpresa,
                IvrProcCuponesGuardarRequest? request)
        {
            return _db.IVR_Proc_Cupones_Eliminar(
                codEmpresa,
                request);
        }

        public ErrorDto
            IVR_Proc_Cupones_Transaccion_Eliminar(
                int codEmpresa,
                IvrProcCuponesTransaccionEliminarRequest?
                    request)
        {
            return _db
                .IVR_Proc_Cupones_Transaccion_Eliminar(
                    codEmpresa,
                    request);
        }
    }
}