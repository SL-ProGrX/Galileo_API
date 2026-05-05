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
    public class FrmVivRegistroAvaluoController : ControllerBase
    {
        private readonly FrmVivRegistroAvaluoBl _bl;

        public FrmVivRegistroAvaluoController(IConfiguration config)
        {
            _bl = new FrmVivRegistroAvaluoBl(config);
        }


        [HttpPost("Viv_GarantiaAvaluo_Obtener")]
        public ErrorDto<FrmVivGarantiaAvaluoRegistroResponse> Viv_GarantiaAvaluo_Obtener(
    int codEmpresa,
    FrmVivGarantiaAvaluoRegistroRequest request)
        {
            return _bl.Viv_GarantiaAvaluo_Obtener(codEmpresa, request);
        }

        [HttpPost("Viv_GarantiaAvaluo_Guardar")]
        public ErrorDto Viv_GarantiaAvaluo_Guardar(
            int codEmpresa,
            FrmVivGarantiaAvaluoGuardarRequest request)
        {
            return _bl.Viv_GarantiaAvaluo_Guardar(codEmpresa, request);
        }

        [HttpPost("Viv_GarantiaAvaluoMonto_Guardar")]
        public ErrorDto<FrmVivGarantiaAvaluoMontoCambiarResponse> Viv_GarantiaAvaluoMonto_Guardar(
            int codEmpresa,
            FrmVivGarantiaAvaluoMontoCambiarRequest request)
        {
            return _bl.Viv_GarantiaAvaluoMonto_Guardar(codEmpresa, request);
        }

    }
}
