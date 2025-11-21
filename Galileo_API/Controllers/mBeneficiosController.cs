using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MBeneficiosController : ControllerBase
    {
        readonly MBeneficiosBl mBeneficioBL;

        public MBeneficiosController(IConfiguration config)
        {
            mBeneficioBL = new MBeneficiosBl(config);
        }

        [HttpGet("fxNombre")]
        public ErrorDto fxNombre(int CodEmpresa, string cedula)
        {
            return mBeneficioBL.fxNombre(CodEmpresa, cedula);
        }

        [HttpGet("fxDescribeBanco")]
        public ErrorDto fxDescribeBanco(int CodEmpresa, int codBanco)
        {
            return mBeneficioBL.fxDescribeBanco(CodEmpresa, codBanco);
        }
    }
}
