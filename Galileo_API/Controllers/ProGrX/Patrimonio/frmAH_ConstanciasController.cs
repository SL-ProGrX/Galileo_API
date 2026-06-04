using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Patrimonio;
using Galileo_API.DataBaseTier.ProGrX.Patrimonio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Patrimonio
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmAHConstanciasController : ControllerBase
    {
        private readonly FrmAHConstanciasBL _bl;

        public FrmAHConstanciasController(IConfiguration config)
        {
            _bl = new FrmAHConstanciasBL(config);
        }

        [HttpGet("Patrimonio_frmAH_Constancias_Consulta_Obtener")]
        public ErrorDto<FrmAhConstanciasConsultaResponse?> Patrimonio_frmAH_Constancias_Consulta_Obtener(
            [FromQuery] int CodEmpresa,
            [FromQuery] string Cedula,
            [FromQuery] string Usuario)
            => _bl.Patrimonio_frmAH_Constancias_Consulta_Obtener(CodEmpresa, Cedula, Usuario);
    }
}
