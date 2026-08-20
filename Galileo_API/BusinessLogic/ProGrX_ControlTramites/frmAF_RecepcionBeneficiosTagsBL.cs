using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_ControlTramites;
using Galileo_API.Models.ProGrX_ControlTramites;

namespace Galileo_API.BusinessLogic.ProGrX_ControlTramites
{
    public sealed class FrmAfRecepcionBeneficiosTagsBl
    {
        private readonly FrmAfRecepcionBeneficiosTagsDb _db;

        public FrmAfRecepcionBeneficiosTagsBl(IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            _db = new FrmAfRecepcionBeneficiosTagsDb(config);
        }

        public ErrorDto<AfRecepcionBeneficiosTagsInicializarResponse>
            AF_frmAF_RecepcionBeneficiosTags_Inicializar(int codEmpresa)
        {
            return _db.AF_frmAF_RecepcionBeneficiosTags_Inicializar(codEmpresa);
        }

        public ErrorDto<AfRecepcionBeneficiosTagsBeneficioResponse?>
            AF_frmAF_RecepcionBeneficiosTags_Beneficio_Obtener(
                int codEmpresa,
                string codBeneficio,
                long consec,
                string movimiento)
        {
            return _db.AF_frmAF_RecepcionBeneficiosTags_Beneficio_Obtener(
                codEmpresa,
                codBeneficio,
                consec,
                movimiento);
        }

        public ErrorDto<List<AfRecepcionBeneficiosTagsPendienteResponse>>
            AF_frmAF_RecepcionBeneficiosTags_Pendientes_Obtener(
                int codEmpresa,
                string movimiento)
        {
            return _db.AF_frmAF_RecepcionBeneficiosTags_Pendientes_Obtener(
                codEmpresa,
                movimiento);
        }

        public ErrorDto<AfRecepcionBeneficiosTagsAplicarResponse>
            AF_frmAF_RecepcionBeneficiosTags_Movimiento_Aplicar(
                int codEmpresa,
                AfRecepcionBeneficiosTagsAplicarRequest request)
        {
            return _db.AF_frmAF_RecepcionBeneficiosTags_Movimiento_Aplicar(
                codEmpresa,
                request);
        }

        public ErrorDto<List<AfRecepcionBeneficiosTagsHistorialResponse>>
            AF_frmAF_RecepcionBeneficiosTags_Historial_Obtener(
                int codEmpresa,
                AfRecepcionBeneficiosTagsHistorialRequest request)
        {
            return _db.AF_frmAF_RecepcionBeneficiosTags_Historial_Obtener(
                codEmpresa,
                request);
        }
    }
}
