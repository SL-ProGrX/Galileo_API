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
        [HttpGet("Cajas_Usuario_Obtener")]
        public ErrorDto<List<CajasUserDto>> Cajas_Usuario_Obtener(int codEmpresa, string usuario)
        {
            return BL_CR_PolizasPSD.Cajas_Usuario_Obtener(codEmpresa, usuario);
        }
        
    }
}