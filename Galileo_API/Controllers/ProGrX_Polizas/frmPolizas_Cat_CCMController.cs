using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Polizas;
using Galileo.Models;
using Galileo_API.Models.ProGrX_Polizas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Galileo_API.Controllers.ProGrX_Polizas
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmPolizasCatCcmController : ControllerBase
    {
        private readonly FrmPolizasCatCcmBL _bl;

        public FrmPolizasCatCcmController(IConfiguration config)
        {
            _bl = new FrmPolizasCatCcmBL(config);
        }

        [Authorize]
        [HttpGet("PolizasCatalogo_Listar")]
        public ErrorDto<List<DropDownListaGenericaModel>> PolizasCatalogo_Listar(int codEmpresa)
        {
            return _bl.PolizasCatalogo_Listar(codEmpresa);
        }

        [Authorize]
        [HttpGet("PolizasConceptosConfigListas")]
        public ErrorDto<List<PolizasCoberturasMotivosCausasDto>> PolizasConceptosConfigListas(
            int codEmpresa, [FromQuery] string codPoliza, [FromQuery] string tipo)
        {
            return _bl.PolizasConceptosConfigListas(codEmpresa, codPoliza, tipo);
        }

        [Authorize]
        [HttpPost("PolizasConceptosConfigAdd")]
        public ErrorDto<PolizasConceptosConfigAddResult> PolizasConceptosConfigAdd(int codEmpresa, [FromBody] PolizasConceptosConfigAddParams param)
        {
            return _bl.PolizasConceptosConfigAdd(codEmpresa, param);
        }

        [Authorize]
        [HttpPost("PolizasConceptosConfigDel")]
        public ErrorDto<PolizasConceptosConfigAddResult> PolizasConceptosConfigDel(int codEmpresa, [FromBody] PolizasConceptosConfigDelParams param)
        {
            return _bl.PolizasConceptosConfigDel(codEmpresa, param);
        }
    }
}
