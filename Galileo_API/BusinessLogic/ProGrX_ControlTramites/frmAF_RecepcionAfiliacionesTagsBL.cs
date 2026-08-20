using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_ControlTramites;
using Galileo_API.Models.ProGrX_ControlTramites;

namespace Galileo_API.BusinessLogic.ProGrX_ControlTramites
{
    public sealed class FrmAfRecepcionAfiliacionesTagsBl
    {
        private readonly FrmAfRecepcionAfiliacionesTagsDb _db;

        public FrmAfRecepcionAfiliacionesTagsBl(IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            _db = new FrmAfRecepcionAfiliacionesTagsDb(config);
        }

        public ErrorDto<AfRecepcionAfiliacionesTagsInicializarResponse>
            AF_frmAF_RecepcionAfiliacionesTags_Inicializar(int codEmpresa)
        {
            return _db.AF_frmAF_RecepcionAfiliacionesTags_Inicializar(
                codEmpresa);
        }

        public ErrorDto<AfRecepcionAfiliacionesTagsMantenimientoResponse>
            AF_frmAF_RecepcionAfiliacionesTags_Pendientes_Antiguos_Eliminar(
                int codEmpresa)
        {
            return _db
                .AF_frmAF_RecepcionAfiliacionesTags_Pendientes_Antiguos_Eliminar(
                    codEmpresa);
        }

        public ErrorDto<List<AfRecepcionAfiliacionesTagsBoletaResponse>>
            AF_frmAF_RecepcionAfiliacionesTags_Boletas_Obtener(
                int codEmpresa,
                string cedula,
                string movimiento)
        {
            return _db.AF_frmAF_RecepcionAfiliacionesTags_Boletas_Obtener(
                codEmpresa,
                cedula,
                movimiento);
        }

        public ErrorDto<AfRecepcionAfiliacionesTagsAfiliacionResponse?>
            AF_frmAF_RecepcionAfiliacionesTags_Afiliacion_Obtener(
                int codEmpresa,
                string cedula,
                long numeroBoleta,
                string movimiento)
        {
            return _db.AF_frmAF_RecepcionAfiliacionesTags_Afiliacion_Obtener(
                codEmpresa,
                cedula,
                numeroBoleta,
                movimiento);
        }

        public ErrorDto<List<AfRecepcionAfiliacionesTagsPendienteResponse>>
            AF_frmAF_RecepcionAfiliacionesTags_Pendientes_Obtener(
                int codEmpresa,
                string movimiento)
        {
            return _db.AF_frmAF_RecepcionAfiliacionesTags_Pendientes_Obtener(
                codEmpresa,
                movimiento);
        }

        public ErrorDto<AfRecepcionAfiliacionesTagsAplicarResponse>
            AF_frmAF_RecepcionAfiliacionesTags_Movimiento_Aplicar(
                int codEmpresa,
                AfRecepcionAfiliacionesTagsAplicarRequest request)
        {
            return _db.AF_frmAF_RecepcionAfiliacionesTags_Movimiento_Aplicar(
                codEmpresa,
                request);
        }

        public ErrorDto<List<AfRecepcionAfiliacionesTagsHistorialResponse>>
            AF_frmAF_RecepcionAfiliacionesTags_Historial_Obtener(
                int codEmpresa,
                AfRecepcionAfiliacionesTagsHistorialRequest request)
        {
            return _db.AF_frmAF_RecepcionAfiliacionesTags_Historial_Obtener(
                codEmpresa,
                request);
        }
    }
}
