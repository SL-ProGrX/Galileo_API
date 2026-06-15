using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FrmCrAprobacionMasivaController : ControllerBase
    {
        private readonly FrmCrAprobacionMasivaBL _bl;

        public FrmCrAprobacionMasivaController(IConfiguration config)
        {
            _bl = new FrmCrAprobacionMasivaBL(config);
        }

        [HttpGet("CrAprobacionMasiva_Consulta_Obtener")]
        public ErrorDto<List<CrAprobacionMasivaOperacionData>> CrAprobacionMasiva_Consulta_Obtener(
            int codEmpresa,
            [FromQuery] CrAprobacionMasivaConsultaRequest request)
            => _bl.CrAprobacionMasiva_Consulta_Obtener(codEmpresa, request);

        [HttpGet("CrAprobacionMasiva_LineasCatalago_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CrAprobacionMasiva_LineasCatalago_Obtener(
            int codEmpresa,
            string? codigo)
            => _bl.CrAprobacionMasiva_LineasCatalago_Obtener(codEmpresa, codigo);

        [HttpPost("CrAprobacionMasiva_Formalizar")]
        public ErrorDto CrAprobacionMasiva_Formalizar(
            int codEmpresa,
            [FromBody] CrAprobacionMasivaFormalizarRequest request)
            => _bl.CrAprobacionMasiva_Formalizar(codEmpresa, request);
    }
}