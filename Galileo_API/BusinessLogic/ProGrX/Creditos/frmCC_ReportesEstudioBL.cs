using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public sealed class FrmCcReportesEstudioBL
    {
        private readonly FrmCcReportesEstudioDB _db;

        public FrmCcReportesEstudioBL(IConfiguration config) => _db = new FrmCcReportesEstudioDB(config);

        public ErrorDto<CcReportesEstudioCatalogosResponseDto> CC_ReportesEstudio_Catalogos_Obtener(int codEmpresa)
            => _db.CC_ReportesEstudio_Catalogos_Obtener(codEmpresa);

        public ErrorDto<List<Dictionary<string, object?>>> CC_ReportesEstudio_Lineas_Obtener(
            int codEmpresa, CcReportesEstudioLineasRequestDto request)
            => _db.CC_ReportesEstudio_Lineas_Obtener(codEmpresa, request);

        public ErrorDto<List<Dictionary<string, object?>>> CC_ReportesEstudio_Resultado_Obtener(
            int codEmpresa, string usuario, CcReportesEstudioGenerarRequestDto request)
            => _db.CC_ReportesEstudio_Resultado_Obtener(codEmpresa, usuario, request);
    }
}
