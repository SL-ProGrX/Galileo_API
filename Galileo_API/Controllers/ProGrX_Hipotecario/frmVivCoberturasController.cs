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
    public class FrmVivCoberturasController : ControllerBase
    {
        private readonly FrmVivCoberturasBl _bl;

        public FrmVivCoberturasController(IConfiguration config)
        {
            _bl = new FrmVivCoberturasBl(config);
        }

        [HttpGet("Viv_Coberturas_Cargar")]
        public ErrorDto<FrmVivCoberturasCargaResponse> Viv_Coberturas_Cargar(
            int codEmpresa,
            long numero_operacion)
        {
            return _bl.Viv_Coberturas_Cargar(codEmpresa, numero_operacion);
        }

        [HttpPost("Viv_CoberturasResumen_Obtener")]
        public ErrorDto<FrmVivCoberturasResumenResponse> Viv_CoberturasResumen_Obtener(
            int codEmpresa,
            FrmVivCoberturasResumenRequest request)
        {
            return _bl.Viv_CoberturasResumen_Obtener(codEmpresa, request);
        }
    }
}
