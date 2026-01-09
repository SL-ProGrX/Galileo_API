using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo_API.BusinessLogic.ProGrX.Bancos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace PgxAPI.Controllers.ProGrX.Bancos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmTesAutorizaChKeyController : ControllerBase
    {
        private readonly IConfiguration? _config;
        private readonly FrmTesAutorizaChKeyBL _authChKeyBL;

        public FrmTesAutorizaChKeyController(IConfiguration config)
        {
            _config = config;
            _authChKeyBL = new FrmTesAutorizaChKeyBL(_config);
        }

        
        [HttpPost("TES_AutorizaChKey_Cambiar")]
        public ErrorDto TES_AutorizaChKey_Cambiar(AutorizaChKeyData usuario)
        {
            return _authChKeyBL.Tes_AutorizaChKey_Cambiar(usuario);
        }

    }
}
