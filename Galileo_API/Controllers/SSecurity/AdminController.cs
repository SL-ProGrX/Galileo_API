using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.Security;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IConfiguration _config;

        public AdminController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost("UsuarioLogin")]
        public LoginResult UsuarioLogin(LoginRequest info)
        {
            return new AdminBL(_config).Login(info.UserName, info.Password);
        }

    }
}