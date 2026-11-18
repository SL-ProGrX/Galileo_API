using Galileo.BusinessLogic;
using Galileo.Models;
using Galileo.Models.ERROR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MProGrxMainController : ControllerBase
    {
        private readonly MProGrxMainBL _bl;

        public MProGrxMainController(IConfiguration config)
        {
            _bl = new MProGrxMainBL(config);
        }

        [HttpGet("ConfiguracionEmpresaSesion_Obtener")]
        public ErrorDto<ConfiguracionEmpresaSesionDto> ConfiguracionEmpresaSesion_Obtener(
            int CodEmpresa,
            string Usuario)
        {
            return _bl.ConfiguracionEmpresaSesion_Obtener(CodEmpresa, Usuario);
        }
    }
}
