using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models;

namespace Galileo.Controllers
{

    [Route("api/[controller]")]
    [ApiController]

    public class SeguridadPortalController : ControllerBase
    {

        private readonly IConfiguration _config;

        public SeguridadPortalController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost("sbAdmin_Rols_Load")]
        public AdminAccessDto sbAdmin_Rols_Load(string pUsuario, int EmpresaId)
        {
            return new SeguridadPortalBl(_config).sbAdmin_Rols_Load(pUsuario, EmpresaId);
        }


        [HttpPost("sbSIFMenuOptionClick")]
        public string sbSIFMenuOptionClick(int pNodo, int Cliente, string Usuario)
        {
            return new SeguridadPortalBl(_config).sbSIFMenuOptionClick(pNodo, Cliente, Usuario);
        }
    }
}