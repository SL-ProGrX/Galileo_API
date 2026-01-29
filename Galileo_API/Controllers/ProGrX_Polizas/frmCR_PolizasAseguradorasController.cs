using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo_API.BusinessLogic.ProGrX_Polizas;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Polizas;

namespace Galileo_API.Controllers.ProGrX_Polizas
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCRPolizasAseguradorasController : ControllerBase
    {
        private readonly FrmCRPolizasAseguradorasBl BL_CR_PolizasAseguradoras;
        public FrmCRPolizasAseguradorasController(IConfiguration config)
        {
            BL_CR_PolizasAseguradoras = new FrmCRPolizasAseguradorasBl(config);
        }

        [Authorize]
        [HttpGet("Poliza_PSD_Consulta")]
        public ErrorDto<List<PolizaAseguradoraDto>> Poliza_PSD_Consulta(int codEmpresa, DateTime fechaCorte, string usuario, string tipo)
        {
            return BL_CR_PolizasAseguradoras.Poliza_PSD_Consulta(codEmpresa, fechaCorte, usuario, tipo);
        }

    }
}
