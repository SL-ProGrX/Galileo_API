using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Patrimonio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Patrimonio
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmAhRegistraAhorroController : ControllerBase
    {
        private readonly FrmAhRegistraAhorroBL _bl;

        public FrmAhRegistraAhorroController(IConfiguration config)
        {
            _bl = new FrmAhRegistraAhorroBL(config);
        }

        [HttpGet("AH_RegistraAhorro_Cargar")]
        public ErrorDto<FrmAhRegistraAhorroCargarResponse> AH_RegistraAhorro_Cargar(
            [FromQuery] int codEmpresa,
            [FromQuery] FrmAhRegistraAhorroCargarRequest request)
        {
            return _bl.AH_RegistraAhorro_Cargar(codEmpresa, request);
        }

        [HttpPost("AH_RegistraAhorro_Gestion_Registrar")]
        public ErrorDto<FrmAhRegistraAhorroGestionResponse> AH_RegistraAhorro_Gestion_Registrar(
            [FromQuery] int codEmpresa,
            [FromBody] FrmAhRegistraAhorroGestionRegistrarRequest request)
        {
            return _bl.AH_RegistraAhorro_Gestion_Registrar(codEmpresa, request);
        }

        [HttpGet("AH_RegistraAhorro_Gestion_Estado")]
        public ErrorDto<FrmAhRegistraAhorroGestionResponse> AH_RegistraAhorro_Gestion_Estado(
            [FromQuery] int codEmpresa,
            [FromQuery] int gestionId)
        {
            return _bl.AH_RegistraAhorro_Gestion_Estado(codEmpresa, gestionId);
        }

        [HttpPost("AH_RegistraAhorro_Aplicar")]
        public ErrorDto<FrmAhRegistraAhorroAplicarResponse> AH_RegistraAhorro_Aplicar(
            [FromQuery] int codEmpresa,
            [FromBody] FrmAhRegistraAhorroAplicarRequest request)
        {
            return _bl.AH_RegistraAhorro_Aplicar(codEmpresa, request);
        }
    }
}
