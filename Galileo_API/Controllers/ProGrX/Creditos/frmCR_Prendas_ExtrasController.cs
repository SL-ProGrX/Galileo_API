using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Credito;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCrPrendasExtrasController : ControllerBase
    {
        private readonly FrmCrPrendasExtrasBl _bl;

        public FrmCrPrendasExtrasController(IConfiguration config)
        {
            _bl = new FrmCrPrendasExtrasBl(config);
        }

        [HttpGet("CR_Prendas_Extras_Consulta")]
        public ErrorDto<CrPrendasExtrasConsultaData> CR_Prendas_Extras_Consulta(int codEmpresa, long prendaId)
            => _bl.CR_Prendas_Extras_Consulta(codEmpresa, prendaId);

        [HttpPost("CR_Prendas_Extras_Guardar")]
        public ErrorDto<CrPrendasExtrasGuardarData> CR_Prendas_Extras_Guardar(
            int codEmpresa,
            [FromBody] CrPrendasExtrasGuardarRequest request)
            => _bl.CR_Prendas_Extras_Guardar(codEmpresa, request);
    }
}
