using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Contabilidad
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public sealed class FrmCntXAreasReportesController :
        ControllerBase
    {
        private readonly FrmCntXAreasReportesBl _bl;

        public FrmCntXAreasReportesController(
            IConfiguration config)
        {
            _bl = new FrmCntXAreasReportesBl(config);
        }

        [HttpGet(
            "CntX_frmCntX_AreasReportes_Parametros_Obtener")]
        public ErrorDto<DefMascarasDto>
            CntX_frmCntX_AreasReportes_Parametros_Obtener(
                int codEmpresa)
        {
            return _bl
                .CntX_frmCntX_AreasReportes_Parametros_Obtener(
                    codEmpresa);
        }

        [HttpGet(
            "CntX_frmCntX_AreasReportes_Areas_Obtener")]
        public ErrorDto<List<CntXAreasReportesAreaData>>
            CntX_frmCntX_AreasReportes_Areas_Obtener(
                int codEmpresa)
        {
            return _bl
                .CntX_frmCntX_AreasReportes_Areas_Obtener(
                    codEmpresa);
        }

        [HttpGet(
            "CntX_frmCntX_AreasReportes_Area_Obtener")]
        public ErrorDto<CntXAreasReportesAreaData?>
            CntX_frmCntX_AreasReportes_Area_Obtener(
                int codEmpresa,
                int codArea)
        {
            return _bl
                .CntX_frmCntX_AreasReportes_Area_Obtener(
                    codEmpresa,
                    codArea);
        }

        [ValidateAntiForgeryToken]
        [HttpPost(
            "CntX_frmCntX_AreasReportes_Generar")]
        public ErrorDto<CntXAreasReportesGenerarData>
            CntX_frmCntX_AreasReportes_Generar(
                int codEmpresa,
                [FromBody]
                CntXAreasReportesGenerarRequest? request)
        {
            return _bl
                .CntX_frmCntX_AreasReportes_Generar(
                    codEmpresa,
                    request);
        }
    }
}