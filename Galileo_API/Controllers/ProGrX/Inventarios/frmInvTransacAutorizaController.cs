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
    public class FrmInvTransacAutorizaController : ControllerBase
    {
        private readonly FrmInvTransacAutorizaBL _bl;
        public FrmInvTransacAutorizaController(IConfiguration config)
        {
            _bl = new FrmInvTransacAutorizaBL(config);
        }

        [HttpPost("InvTransacAutoriza")]
        public ErrorDto InvTransacAutoriza_Actualizar(int CodEmpresa, InvTransacAutoriza request)
        {
            return _bl.InvTransacAutoriza_Actualizar(CodEmpresa, request);
        }
    }
}