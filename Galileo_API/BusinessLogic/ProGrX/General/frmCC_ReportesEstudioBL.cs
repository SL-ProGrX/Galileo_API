using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.General;
using Galileo_API.Models.ProGrX.General;

namespace Galileo_API.BusinessLogic.ProGrX.General
{
    public class FrmCcReportesEstudioBL
    {
        private readonly FrmCcReportesEstudioDB _Db;

        public FrmCcReportesEstudioBL(IConfiguration config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            _Db = new FrmCcReportesEstudioDB(config);
        }

        public ErrorDto<CcReportesEstudioCatalogosResponseDto> CC_ReportesEstudio_Catalogos_Obtener(int codEmpresa)
        {
            return _Db.CC_ReportesEstudio_Catalogos_Obtener(codEmpresa);
        }

        public ErrorDto<List<Dictionary<string, object?>>> CC_ReportesEstudio_Lineas_Obtener(
            int codEmpresa, CcReportesEstudioLineasRequestDto request)
        {
            return _Db.CC_ReportesEstudio_Lineas_Obtener(codEmpresa, request);
        }

        public ErrorDto<List<Dictionary<string, object?>>> CC_ReportesEstudio_Resultado_Obtener(
            int codEmpresa, string usuario, CcReportesEstudioGenerarRequestDto request)
        {
            return _Db.CC_ReportesEstudio_Resultado_Obtener(codEmpresa, usuario, request);
        }
    }
}
