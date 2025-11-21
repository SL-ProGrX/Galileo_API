using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models;
using Galileo.Models.ERROR;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class mBeneficiosController : Controller
    {
        private readonly IConfiguration _config;
        mBeneficiosBL mBeneficioBL;

        public mBeneficiosController(IConfiguration config)
        {
            _config = config;
            mBeneficioBL = new mBeneficiosBL(_config);
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
