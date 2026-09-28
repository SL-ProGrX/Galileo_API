using Galileo.BusinessLogic;
using Galileo.Models;
using Galileo.Models.ERROR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public sealed class FrmInvTransacReportesController : ControllerBase
    {
        private readonly FrmInvTransacReportesBL _bl;

        public FrmInvTransacReportesController(IConfiguration config)
        {
            _bl = new FrmInvTransacReportesBL(config);
        }

        [HttpGet("INV_TransacReportes_Causas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>>
            INV_TransacReportes_Causas_Obtener(int CodEmpresa, string tipo)
        {
            return _bl.INV_TransacReportes_Causas_Obtener(CodEmpresa, tipo);
        }

        [HttpGet("INV_TransacReportes_Usuarios_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>>
            INV_TransacReportes_Usuarios_Obtener(int CodEmpresa)
        {
            return _bl.INV_TransacReportes_Usuarios_Obtener(CodEmpresa);
        }
    }
}
