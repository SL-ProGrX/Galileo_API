using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Hipotecario;
using Galileo_API.Models.ProGrX_Hipotecario;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Hipotecario
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmVivParametrosController : ControllerBase
    {
        private readonly FrmVivParametrosBl _bl;

        public FrmVivParametrosController(IConfiguration config)
        {
            _bl = new FrmVivParametrosBl(config);
        }

        [HttpGet("VivParametros_Obtener")]
        public ErrorDto<List<VivParametrosData>> VivParametros_Obtener(int codEmpresa)
        {
            return _bl.VivParametros_Obtener(codEmpresa);
        }

        [HttpGet("VivTiposDesembolsos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> VivTiposDesembolsos_Obtener(int codEmpresa)
        {
            return _bl.VivTiposDesembolsos_Obtener(codEmpresa);
        }

        [HttpPost("VivParametros_Guardar")]
        public ErrorDto VivParametros_Guardar(int codEmpresa, string usuario, VivParametrosData request)
        {
            return _bl.VivParametros_Guardar(codEmpresa, usuario, request);
        }
    }
}
