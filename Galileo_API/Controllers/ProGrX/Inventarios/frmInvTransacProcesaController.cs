using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using Microsoft.AspNetCore.Authorization;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class FrmInvTransacProcesaController : ControllerBase
    {
        private readonly FrmInvTransacProcesaBL _bl;

        public FrmInvTransacProcesaController(IConfiguration config)
        {
            _bl = new FrmInvTransacProcesaBL(config);
        }

        [HttpPost("InvTransacProcesa")]
        public ErrorDto InvTransacProcesa(int CodEmpresa, InvTransacProcesa request)
        {
            return _bl.InvTransacProcesa_SP(CodEmpresa, request);
        }
    }
}