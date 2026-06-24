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
    public class FrmInvTransacQryController : ControllerBase
    {
        private readonly FrmInvTransacQryBL _bl;

        public FrmInvTransacQryController(IConfiguration config)
        {
            _bl = new FrmInvTransacQryBL(config);
        }

        [HttpPost("TransacInv_Obtener")]
        public ErrorDto<TransacQryDataList> TransacInv_Obtener(int CodEmpresa, TransacQryParametros parametros)
        {
            return _bl.TransacInv_Obtener(CodEmpresa, parametros);
        }
    }
}