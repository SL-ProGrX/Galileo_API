using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrPlazosBonificacionBl
    {
        private readonly FrmCrPlazosBonificacionDb _db;

        public FrmCrPlazosBonificacionBl(IConfiguration config)
        {
            _db = new FrmCrPlazosBonificacionDb(config);
        }

        public ErrorDto<List<CrPlazosBonificacionPlanData>> CrPlazosBonificacion_Planes_Obtener(int codEmpresa)
            => _db.CrPlazosBonificacion_Planes_Obtener(codEmpresa);

        public ErrorDto<CrPlazosBonificacionDefinicionData?> CrPlazosBonificacion_Scroll_Obtener(
            int codEmpresa, int scroll, string codPlazoBono)
            => _db.CrPlazosBonificacion_Scroll_Obtener(codEmpresa, scroll, codPlazoBono);

        public ErrorDto<CrPlazosBonificacionDefinicionData?> CrPlazosBonificacion_Definicion_Obtener(
            int codEmpresa,
            string codPlazoBono)
            => _db.CrPlazosBonificacion_Definicion_Obtener(codEmpresa, codPlazoBono);

        public ErrorDto CrPlazosBonificacion_Definicion_Guardar(
            int codEmpresa,
            CrPlazosBonificacionDefinicionGuardarRequest request)
            => _db.CrPlazosBonificacion_Definicion_Guardar(codEmpresa, request);

        public ErrorDto CrPlazosBonificacion_Definicion_Eliminar(
            int codEmpresa,
            CrPlazosBonificacionDefinicionEliminarRequest request)
            => _db.CrPlazosBonificacion_Definicion_Eliminar(codEmpresa, request);

        public ErrorDto<List<CrPlazosBonificacionBonificacionData>> CrPlazosBonificacion_Bonificaciones_Obtener(
            int codEmpresa,
            string codPlazoBono)
            => _db.CrPlazosBonificacion_Bonificaciones_Obtener(codEmpresa, codPlazoBono);

        public ErrorDto CrPlazosBonificacion_Bonificaciones_Guardar(
            int codEmpresa,
            CrPlazosBonificacionBonificacionGuardarRequest request)
            => _db.CrPlazosBonificacion_Bonificaciones_Guardar(codEmpresa, request);

        public ErrorDto CrPlazosBonificacion_Bonificaciones_Eliminar(
            int codEmpresa,
            CrPlazosBonificacionBonificacionEliminarRequest request)
            => _db.CrPlazosBonificacion_Bonificaciones_Eliminar(codEmpresa, request);

        public ErrorDto<List<CrPlazosBonificacionAsignacionData>> CrPlazosBonificacion_Asignaciones_Obtener(
            int codEmpresa,
            string codPlazoBono)
            => _db.CrPlazosBonificacion_Asignaciones_Obtener(codEmpresa, codPlazoBono);

        public ErrorDto CrPlazosBonificacion_Asignaciones_Guardar(
            int codEmpresa,
            CrPlazosBonificacionAsignacionGuardarRequest request)
            => _db.CrPlazosBonificacion_Asignaciones_Guardar(codEmpresa, request);
    }
}