using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo_API.BusinessLogic.ProGrX_Polizas;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Polizas;

namespace Galileo_API.Controllers.ProGrX_Polizas
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCRPolizasPsdController : ControllerBase
    {
        private readonly FrmCRPolizasPsdBl BL_CR_PolizasPSD;
        public FrmCRPolizasPsdController(IConfiguration config)
        {
            BL_CR_PolizasPSD = new FrmCRPolizasPsdBl(config);
        }



        [Authorize]
        [HttpGet("Poliza_PSD_Consulta")]
        public ErrorDto<List<PolizaPsdDto>> Poliza_PSD_Consulta(int codEmpresa,DateTime fechaCorte,string usuario,string tipo)
        {
            return BL_CR_PolizasPSD.Poliza_PSD_Consulta(codEmpresa,fechaCorte,usuario,tipo);
        }

        [Authorize]
        [HttpPost("Poliza_PSD_Genera")]
        public ErrorDto<bool> Poliza_PSD_Genera(int codEmpresa,DateTime fechaCorte,string usuario)
        {
            return BL_CR_PolizasPSD.Poliza_PSD_Genera(codEmpresa,fechaCorte,usuario
            );
        }
    }
}
