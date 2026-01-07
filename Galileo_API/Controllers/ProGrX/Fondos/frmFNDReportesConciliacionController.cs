using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Fondos;
using Galileo_API.Models.ProGrX.Fondos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Fondos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmFndReportesConciliacionController : ControllerBase
    {
        private readonly FrmFndReportesConciliacionBL _bl;

        public FrmFndReportesConciliacionController(IConfiguration config)
        {
            _bl = new FrmFndReportesConciliacionBL(config);
        }

        [Authorize]
        [HttpGet("ReportesConciliacion_Operadoras_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> ReportesConciliacion_Operadoras_Obtener(int codEmpresa)
        {
            return _bl.ReportesConciliacion_Operadoras_Obtener(codEmpresa);
        }

        [Authorize]
        [HttpGet("ReportesConciliacion_Entidades_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> ReportesConciliacion_Entidades_Obtener(int codEmpresa)
        {
            return _bl.ReportesConciliacion_Entidades_Obtener(codEmpresa);
        }

        [Authorize]
        [HttpGet("ReportesConciliacion_PeriodosHistorico_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> ReportesConciliacion_PeriodosHistorico_Obtener(int codEmpresa)
        {
            return _bl.ReportesConciliacion_PeriodosHistorico_Obtener(codEmpresa);
        }

        [Authorize]
        [HttpGet("ReportesConciliacion_PeriodoHistoricoDetalle_Obtener")]
        public ErrorDto<FndPerHistoricoDetalleModel?> ReportesConciliacion_PeriodoHistoricoDetalle_Obtener(int codEmpresa, string idPerHistorico)
        {
            return _bl.ReportesConciliacion_PeriodoHistoricoDetalle_Obtener(codEmpresa, idPerHistorico);
        }
    }

}
