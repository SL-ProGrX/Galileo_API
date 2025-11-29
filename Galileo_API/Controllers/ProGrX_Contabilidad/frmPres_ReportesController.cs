using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.PRES;

namespace Galileo.Controllers
{
    [Route("api/frmPres_Reportes")]
    [Route("api/FrmPresReportes")]
    [ApiController]
    public class FrmPresReportesController : ControllerBase
    {
        readonly FrmPresReportesBl _bl;
        public FrmPresReportesController(IConfiguration config)
        {
            _bl = new FrmPresReportesBl(config);
        }

        [HttpGet("fxPres_Periodo_Obtener")]
        // [Authorize]
        public ErrorDto<List<ModeloGenericList>> fxPres_Periodo_Obtener(int CodEmpresa, int CodContab, string CodModelo)
        {
            return _bl.fxPres_Periodo_Obtener(CodEmpresa, CodContab, CodModelo);
        }

        [HttpGet("spPres_Ajustes_Permitidos_Obtener")]
        // [Authorize]
        public ErrorDto<List<ModeloGenericList>> spPres_Ajustes_Permitidos_Obtener(int CodEmpresa, int codContab, string codModelo, string Usuario)
        {
            return _bl.spPres_Ajustes_Permitidos_Obtener(CodEmpresa, codContab, codModelo, Usuario);
        }
    }
}