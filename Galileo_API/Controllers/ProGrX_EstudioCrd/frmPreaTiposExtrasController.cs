using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_EstudioCrd;
using Galileo_API.Models.ProGrX_EstudioCrd;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_EstudioCrd
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmPreaTiposExtrasController : ControllerBase
    {
        private readonly FrmPreaTiposExtrasBl _bl;

        public FrmPreaTiposExtrasController(IConfiguration config) =>
            _bl = new FrmPreaTiposExtrasBl(config);

        [HttpGet("CrPreaTiposExtras_Obtener")]
        public ErrorDto<List<CrdPreaTiposExtrasData>> CrPreaTiposExtras_Obtener(int codEmpresa)
        {
            return _bl.CrPreaTiposExtras_Obtener(codEmpresa);
        }

        [HttpPost("CrPreaTiposExtras_Guardar")]
        public ErrorDto CrPreaTiposExtras_Guardar(int codEmpresa, string usuario, CrdPreaTiposExtrasData request)
        {
            return _bl.CrPreaTiposExtras_Guardar(codEmpresa, usuario, request);
        }

        [HttpDelete("CrPreaTiposExtras_Eliminar")]
        public ErrorDto CrPreaTiposExtras_Eliminar(int codEmpresa, string codExtra, string usuario)
        {
            return _bl.CrPreaTiposExtras_Eliminar(codEmpresa, codExtra, usuario);
        }
    }
}