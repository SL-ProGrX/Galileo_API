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
    public class FrmInvTranReversionController : ControllerBase
    {
        private readonly FrmInvTranReversionBL _bl;
        public FrmInvTranReversionController(IConfiguration config)
        {
            _bl = new FrmInvTranReversionBL(config);
        }

        [HttpGet("InvTranReversion_Obtener")]
        public ErrorDto<TranReversionData> InvTranReversion_Obtener(int CodEmpresa, string CodBoleta, string TipoTran)
        {
            return _bl.InvTranReversion_Obtener(CodEmpresa, CodBoleta, TipoTran);
        }

        [HttpGet("InvProducLineas_Obtener")]
        public ErrorDto<List<InvProducReversion>> InvProducLineas_Obtener(int CodEmpresa, string CodBoleta, string TipoTran)
        {
            return _bl.InvProducLineas_Obtener(CodEmpresa, CodBoleta, TipoTran);
        }

        [HttpGet("InvTranReversion_scroll")]
        public ErrorDto<TranReversionData> InvTranReversion_scroll(int CodEmpresa, int scrollValue, string? CodBoleta, string TipoTran)
        {
            return _bl.InvTranReversion_scroll(CodEmpresa, scrollValue, CodBoleta, TipoTran);
        }

        [HttpPost("InvTranReversion_Insertar")]
        public ErrorDto InvTranReversion_Insertar(int CodEmpresa, TranReversionInsert request)
        {
            return _bl.InvTranReversion_Insertar(CodEmpresa, request);
        }
    }
}