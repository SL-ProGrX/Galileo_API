using Galileo.Models.AH;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Patrimonio;

namespace Galileo_API.BusinessLogic.ProGrX.Patrimonio
{
    public class FrmAhExcedentesPeriodosBL
    {
        private readonly FrmAhExcedentesPeriodosDB _db;

        public FrmAhExcedentesPeriodosBL(IConfiguration config)
        {
            _db = new FrmAhExcedentesPeriodosDB(config);
        }

        public ErrorDto<List<FrmAhExcedentesPeriodosListaDto>> Patrimonio_frmAH_ExcedentesPeriodos_Lista(int codEmpresa)
        {
            return _db.Patrimonio_frmAH_ExcedentesPeriodos_Lista(codEmpresa);
        }

        public ErrorDto<FrmAhExcedentesPeriodosDetalleDto> Patrimonio_frmAH_ExcedentesPeriodos_Obtener(
            int codEmpresa,
            int periodoId)
        {
            return _db.Patrimonio_frmAH_ExcedentesPeriodos_Obtener(codEmpresa, periodoId);
        }

        public ErrorDto<List<BitacoraExcedenteDto>> Patrimonio_frmAH_ExcedentesPeriodos_Bitacora_Lista(
            int codEmpresa,
            int periodoId,
            string etapa)
        {
            return _db.Patrimonio_frmAH_ExcedentesPeriodos_Bitacora_Lista(codEmpresa, periodoId, etapa);
        }

        public ErrorDto<List<FrmAhExcedentesPeriodosResumenDto>> Patrimonio_frmAH_ExcedentesPeriodos_Resumen_Lista(
            int codEmpresa,
            int periodoId)
        {
            return _db.Patrimonio_frmAH_ExcedentesPeriodos_Resumen_Lista(codEmpresa, periodoId);
        }

        public ErrorDto<FrmAhExcedentesPeriodosGuardarResponse> Patrimonio_frmAH_ExcedentesPeriodos_Insertar(
            int codEmpresa,
            FrmAhExcedentesPeriodosGuardarRequest request)
        {
            return _db.Patrimonio_frmAH_ExcedentesPeriodos_Insertar(codEmpresa, request);
        }

        public ErrorDto<FrmAhExcedentesPeriodosGuardarResponse> Patrimonio_frmAH_ExcedentesPeriodos_Actualizar(
            int codEmpresa,
            FrmAhExcedentesPeriodosGuardarRequest request)
        {
            return _db.Patrimonio_frmAH_ExcedentesPeriodos_Actualizar(codEmpresa, request);
        }

        public ErrorDto<bool> Patrimonio_frmAH_ExcedentesPeriodos_Eliminar(
            int codEmpresa,
            int periodoId,
            string usuario)
        {
            return _db.Patrimonio_frmAH_ExcedentesPeriodos_Eliminar(codEmpresa, periodoId, usuario);
        }

        public ErrorDto<bool> Patrimonio_frmAH_ExcedentesPeriodos_BaseAplicacion_Actualizar(
            int codEmpresa,
            FrmAhExcedentesPeriodosBaseAplicacionRequest request)
        {
            return _db.Patrimonio_frmAH_ExcedentesPeriodos_BaseAplicacion_Actualizar(codEmpresa, request);
        }

        public ErrorDto<bool> Patrimonio_frmAH_ExcedentesPeriodos_EstadoNota_Actualizar(
            int codEmpresa,
            FrmAhExcedentesPeriodosEstadoNotaRequest request)
        {
            return _db.Patrimonio_frmAH_ExcedentesPeriodos_EstadoNota_Actualizar(codEmpresa, request);
        }

        public ErrorDto<bool> Patrimonio_frmAH_ExcedentesPeriodos_RecalcularBase(
            int codEmpresa,
            FrmAhExcedentesPeriodosRecalcularBaseRequest request)
        {
            return _db.Patrimonio_frmAH_ExcedentesPeriodos_RecalcularBase(codEmpresa, request);
        }
    }
}
