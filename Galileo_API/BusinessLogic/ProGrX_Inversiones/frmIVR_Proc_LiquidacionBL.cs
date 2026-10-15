using Galileo.DataBaseTier.ProGrX_Inversiones;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Inversiones;

namespace Galileo_API.BusinessLogic.ProGrX_Inversiones
{
    public sealed class FrmIvrProcLiquidacionBl
    {
        private readonly FrmIvrProcLiquidacionDb _db;

        public FrmIvrProcLiquidacionBl(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _db = new FrmIvrProcLiquidacionDb(config);
        }

        public ErrorDto<IvrProcLiquidacionInicialData>
            IVR_Proc_Liquidacion_Inicializar(
                int codEmpresa,
                IvrProcLiquidacionInicializarRequest? request)
        {
            return _db.IVR_Proc_Liquidacion_Inicializar(
                codEmpresa,
                request);
        }

        public ErrorDto<IvrProcLiquidacionTransaccionesData>
            IVR_Proc_Liquidacion_Transacciones_Obtener(
                int codEmpresa,
                int transacId)
        {
            return _db
                .IVR_Proc_Liquidacion_Transacciones_Obtener(
                    codEmpresa,
                    transacId);
        }

        public ErrorDto<IvrProcLiquidacionGuardarData>
            IVR_Proc_Liquidacion_Solicitud_Guardar(
                int codEmpresa,
                IvrProcLiquidacionGuardarRequest? request)
        {
            return _db
                .IVR_Proc_Liquidacion_Solicitud_Guardar(
                    codEmpresa,
                    request);
        }

        public ErrorDto<IvrProcLiquidacionGuardarData>
            IVR_Proc_Liquidacion_Procesar(
                int codEmpresa,
                IvrProcLiquidacionGuardarRequest? request)
        {
            return _db.IVR_Proc_Liquidacion_Procesar(
                codEmpresa,
                request);
        }

        public ErrorDto<IvrProcLiquidacionGuardarData>
            IVR_Proc_Liquidacion_Eliminar(
                int codEmpresa,
                IvrProcLiquidacionGuardarRequest? request)
        {
            return _db.IVR_Proc_Liquidacion_Eliminar(
                codEmpresa,
                request);
        }

        public ErrorDto
            IVR_Proc_Liquidacion_Transaccion_Eliminar(
                int codEmpresa,
                IvrProcLiquidacionTransaccionEliminarRequest?
                    request)
        {
            return _db
                .IVR_Proc_Liquidacion_Transaccion_Eliminar(
                    codEmpresa,
                    request);
        }
    }
}