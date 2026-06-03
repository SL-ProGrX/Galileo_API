using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Galileo.BusinessLogic.ProGrX_Personas;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Personas;

namespace Galileo.Controllers.ProGrx_Personas
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAFIngresosConsultaController : ControllerBase
    {
        private readonly FrmAFIngresosConsultaBL _bl;

        public FrmAFIngresosConsultaController(IConfiguration config)
        {
            _bl = new FrmAFIngresosConsultaBL(config);
        }

        [Authorize]
        [HttpPost("AF_Ingresos_Consulta")]
        public ErrorDto<IngresosConsultaLista> AF_Ingresos_Consulta(int CodEmpresa, [FromBody] IngresosConsultaFiltro filtro)
        {
            return _bl.AF_Ingresos_Consulta(CodEmpresa, filtro);
        }
    }
}
