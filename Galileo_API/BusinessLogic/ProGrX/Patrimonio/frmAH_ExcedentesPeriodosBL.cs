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

        public ErrorDto<List<FrmAhExcedentesPeriodosListaDto>> Ah_ExcedentesPeriodos_Lista(int codEmpresa)
        {
            return _db.Ah_ExcedentesPeriodos_Lista(codEmpresa);
        }

        public ErrorDto<FrmAhExcedentesPeriodosDetalleDto> Ah_ExcedentesPeriodos_Obtener(
            int codEmpresa,
            int periodoId)
        {
            return _db.Ah_ExcedentesPeriodos_Obtener(codEmpresa, periodoId);
        }

        public ErrorDto<List<BitacoraExcedenteDto>> Ah_ExcedentesPeriodos_Bitacora_Lista(
            int codEmpresa,
            int periodoId,
            string etapa)
        {
            return _db.Ah_ExcedentesPeriodos_Bitacora_Lista(codEmpresa, periodoId, etapa);
        }

        public ErrorDto<List<FrmAhExcedentesPeriodosResumenDto>> Ah_ExcedentesPeriodos_Resumen_Lista(
            int codEmpresa,
            int periodoId)
        {
            return _db.Ah_ExcedentesPeriodos_Resumen_Lista(codEmpresa, periodoId);
        }

        public ErrorDto<FrmAhExcedentesPeriodosGuardarResponse> Ah_ExcedentesPeriodos_Insertar(
            int codEmpresa,
            FrmAhExcedentesPeriodosGuardarRequest request)
        {
            return _db.Ah_ExcedentesPeriodos_Insertar(codEmpresa, request);
        }

        public ErrorDto<FrmAhExcedentesPeriodosGuardarResponse> Ah_ExcedentesPeriodos_Actualizar(
            int codEmpresa,
            FrmAhExcedentesPeriodosGuardarRequest request)
        {
            return _db.Ah_ExcedentesPeriodos_Actualizar(codEmpresa, request);
        }

        public ErrorDto<bool> Ah_ExcedentesPeriodos_Eliminar(
            int codEmpresa,
            int periodoId,
            string usuario)
        {
            return _db.Ah_ExcedentesPeriodos_Eliminar(codEmpresa, periodoId, usuario);
        }

        public ErrorDto<bool> Ah_ExcedentesPeriodos_BaseAplicacion_Actualizar(
            int codEmpresa,
            FrmAhExcedentesPeriodosBaseAplicacionRequest request)
        {
            return _db.Ah_ExcedentesPeriodos_BaseAplicacion_Actualizar(codEmpresa, request);
        }

        public ErrorDto<bool> Ah_ExcedentesPeriodos_EstadoNota_Actualizar(
            int codEmpresa,
            FrmAhExcedentesPeriodosEstadoNotaRequest request)
        {
            return _db.Ah_ExcedentesPeriodos_EstadoNota_Actualizar(codEmpresa, request);
        }

        public ErrorDto<bool> Ah_ExcedentesPeriodos_RecalcularBase(
            int codEmpresa,
            FrmAhExcedentesPeriodosRecalcularBaseRequest request)
        {
            return _db.Ah_ExcedentesPeriodos_RecalcularBase(codEmpresa, request);
        }
    }
}
