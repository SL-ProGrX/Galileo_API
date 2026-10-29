using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public sealed class FrmCntXAreasReportesBl
    {
        private readonly FrmCntXAreasReportesDb _db;

        public FrmCntXAreasReportesBl(
            IConfiguration config)
        {
            _db = new FrmCntXAreasReportesDb(config);
        }

        public ErrorDto<DefMascarasDto>
            CntX_frmCntX_AreasReportes_Parametros_Obtener(
                int codEmpresa)
        {
            return _db
                .CntX_frmCntX_AreasReportes_Parametros_Obtener(
                    codEmpresa);
        }

        public ErrorDto<List<CntXAreasReportesAreaData>>
            CntX_frmCntX_AreasReportes_Areas_Obtener(
                int codEmpresa)
        {
            return _db
                .CntX_frmCntX_AreasReportes_Areas_Obtener(
                    codEmpresa);
        }

        public ErrorDto<CntXAreasReportesAreaData?>
            CntX_frmCntX_AreasReportes_Area_Obtener(
                int codEmpresa,
                int codArea)
        {
            return _db
                .CntX_frmCntX_AreasReportes_Area_Obtener(
                    codEmpresa,
                    codArea);
        }

        public ErrorDto<CntXAreasReportesGenerarData>
            CntX_frmCntX_AreasReportes_Generar(
                int codEmpresa,
                CntXAreasReportesGenerarRequest? request)
        {
            return _db
                .CntX_frmCntX_AreasReportes_Generar(
                    codEmpresa,
                    request);
        }
    }
}