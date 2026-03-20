using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Cobros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Galileo_API.Models.ProGrX.Cobros.FrmCOReversionCobroJudicialModels;

namespace Galileo_API.Controllers.ProGrX.Cobros
{
    [Route("api/[controller]")]
    [ApiController]


    public class FrmCOReversionCobroJudicialController : ControllerBase
    {
        private readonly FrmCOReversionCobroJudicialBL _bl;

        public FrmCOReversionCobroJudicialController(IConfiguration config)
            => _bl = new FrmCOReversionCobroJudicialBL(config);

        [Authorize]
        [HttpGet("Crd_ReversionCobroJudicial_Consultar")]
        public ErrorDto<CrdReversionCobroJudicialConsultaResponse> Crd_ReversionCobroJudicial_Consultar(int codEmpresa, string usuario, int codContabilidad, int operacion)
         => _bl.Crd_ReversionCobroJudicial_Consultar(codEmpresa, usuario, codContabilidad, operacion);

        [Authorize]
        [HttpPost("Crd_ReversionCobroJudicial_Reversar")]
        public ErrorDto<object> Crd_ReversionCobroJudicial_Reversar(int codEmpresa, [FromBody] CrdReversionCobroJudicialReversaRequest request)
             => _bl.Crd_ReversionCobroJudicial_Reversar(codEmpresa, request);

    }
}
