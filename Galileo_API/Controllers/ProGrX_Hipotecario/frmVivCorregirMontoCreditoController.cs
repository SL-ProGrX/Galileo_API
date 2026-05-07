using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Hipotecario;
using Galileo_API.Models.ProGrX_Hipotecario;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Hipotecario
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmVivCorregirMontoCreditoController : ControllerBase
    {
        private readonly FrmVivCorregirMontoCreditoBL _bl;

        public FrmVivCorregirMontoCreditoController(IConfiguration config)
        {
            _bl = new FrmVivCorregirMontoCreditoBL(config);
        }

        [HttpGet("Viv_CorregirMontoCredito_Obtener")]
        public ErrorDto<FrmVivCorregirMontoCreditoResponse> Viv_CorregirMontoCredito_Obtener(
            int codEmpresa,
            long numero_operacion)
        {
            return _bl.Viv_CorregirMontoCredito_Obtener(codEmpresa, numero_operacion);
        }

        [HttpPost("Viv_CorregirMontoCredito_Guardar")]
        public ErrorDto<FrmVivCorregirMontoCreditoGuardarResponse> Viv_CorregirMontoCredito_Guardar(
            int codEmpresa,
            FrmVivCorregirMontoCreditoGuardarRequest request)
        {
            return _bl.Viv_CorregirMontoCredito_Guardar(codEmpresa, request);
        }
    }
}
