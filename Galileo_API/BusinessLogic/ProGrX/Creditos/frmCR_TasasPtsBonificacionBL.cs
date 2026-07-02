using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrTasasPtsBonificacionBl
    {
        private readonly FrmCrTasasPtsBonificacionDb _db;

        public FrmCrTasasPtsBonificacionBl(IConfiguration config)
        {
            _db = new FrmCrTasasPtsBonificacionDb(config);
        }

        public ErrorDto<List<CrTasasPtsBonificacionPlanData>> CrTasasPtsBonificacion_Planes_Obtener(int codEmpresa)
        {
            return _db.CrTasasPtsBonificacion_Planes_Obtener(codEmpresa);
        }

        public ErrorDto<CrTasasPtsBonificacionDefinicionData?> CrTasasPtsBonificacion_Scroll_Obtener(
            int codEmpresa,
            int scroll,
            string codTasaBono)
        {
            return _db.CrTasasPtsBonificacion_Scroll_Obtener(codEmpresa, scroll, codTasaBono);
        }

        public ErrorDto<CrTasasPtsBonificacionDefinicionData?> CrTasasPtsBonificacion_Definicion_Obtener(
            int codEmpresa,
            string codTasaBono)
        {
            return _db.CrTasasPtsBonificacion_Definicion_Obtener(codEmpresa, codTasaBono);
        }

        public ErrorDto CrTasasPtsBonificacion_Definicion_Guardar(
            int codEmpresa,
            CrTasasPtsBonificacionDefinicionGuardarRequest request)
        {
            return _db.CrTasasPtsBonificacion_Definicion_Guardar(codEmpresa, request);
        }

        public ErrorDto CrTasasPtsBonificacion_Definicion_Eliminar(
            int codEmpresa,
            CrTasasPtsBonificacionDefinicionEliminarRequest request)
        {
            return _db.CrTasasPtsBonificacion_Definicion_Eliminar(codEmpresa, request);
        }

        public ErrorDto<List<CrTasasPtsBonificacionMembresiaData>> CrTasasPtsBonificacion_Membresias_Obtener(
            int codEmpresa,
            string codTasaBono)
        {
            return _db.CrTasasPtsBonificacion_Membresias_Obtener(codEmpresa, codTasaBono);
        }

        public ErrorDto CrTasasPtsBonificacion_Membresias_Guardar(
            int codEmpresa,
            CrTasasPtsBonificacionMembresiaGuardarRequest request)
        {
            return _db.CrTasasPtsBonificacion_Membresias_Guardar(codEmpresa, request);
        }

        public ErrorDto CrTasasPtsBonificacion_Membresias_Eliminar(
            int codEmpresa,
            CrTasasPtsBonificacionLineaEliminarRequest request)
        {
            return _db.CrTasasPtsBonificacion_Membresias_Eliminar(codEmpresa, request);
        }

        public ErrorDto<List<CrTasasPtsBonificacionDestinoData>> CrTasasPtsBonificacion_Destinos_Obtener(
            int codEmpresa,
            string codTasaBono)
        {
            return _db.CrTasasPtsBonificacion_Destinos_Obtener(codEmpresa, codTasaBono);
        }

        public ErrorDto CrTasasPtsBonificacion_Destinos_Guardar(
            int codEmpresa,
            CrTasasPtsBonificacionDestinoGuardarRequest request)
        {
            return _db.CrTasasPtsBonificacion_Destinos_Guardar(codEmpresa, request);
        }

        public ErrorDto CrTasasPtsBonificacion_Destinos_Eliminar(
            int codEmpresa,
            CrTasasPtsBonificacionLineaEliminarRequest request)
        {
            return _db.CrTasasPtsBonificacion_Destinos_Eliminar(codEmpresa, request);
        }

        public ErrorDto<List<CrTasasPtsBonificacionLiquidezData>> CrTasasPtsBonificacion_Liquidez_Obtener(
            int codEmpresa,
            string codTasaBono)
        {
            return _db.CrTasasPtsBonificacion_Liquidez_Obtener(codEmpresa, codTasaBono);
        }

        public ErrorDto CrTasasPtsBonificacion_Liquidez_Guardar(
            int codEmpresa,
            CrTasasPtsBonificacionLiquidezGuardarRequest request)
        {
            return _db.CrTasasPtsBonificacion_Liquidez_Guardar(codEmpresa, request);
        }

        public ErrorDto CrTasasPtsBonificacion_Liquidez_Eliminar(
            int codEmpresa,
            CrTasasPtsBonificacionLineaEliminarRequest request)
        {
            return _db.CrTasasPtsBonificacion_Liquidez_Eliminar(codEmpresa, request);
        }

        public ErrorDto<List<CrTasasPtsBonificacionAsignacionLineaData>> CrTasasPtsBonificacion_Asignaciones_Obtener(
            int codEmpresa,
            string codTasaBono)
        {
            return _db.CrTasasPtsBonificacion_Asignaciones_Obtener(codEmpresa, codTasaBono);
        }

        public ErrorDto<List<CrTasasPtsBonificacionAsignacionPlanData>> CrTasasPtsBonificacion_AsignacionPlanes_Obtener(
            int codEmpresa,
            string codigo,
            string garantia)
        {
            return _db.CrTasasPtsBonificacion_AsignacionPlanes_Obtener(codEmpresa, codigo, garantia);
        }

        public ErrorDto CrTasasPtsBonificacion_Asignaciones_Guardar(
            int codEmpresa,
            CrTasasPtsBonificacionAsignacionGuardarRequest request)
        {
            return _db.CrTasasPtsBonificacion_Asignaciones_Guardar(codEmpresa, request);
        }

        public ErrorDto<List<CrTasasPtsBonificacionDestinoCatalogoData>> CrTasasPtsBonificacion_DestinosCatalogo_Obtener(
            int codEmpresa)
        {
            return _db.CrTasasPtsBonificacion_DestinosCatalogo_Obtener(codEmpresa);
        }
    }
}
