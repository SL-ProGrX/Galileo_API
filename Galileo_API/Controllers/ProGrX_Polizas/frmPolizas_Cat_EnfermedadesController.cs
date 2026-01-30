using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Polizas;
using Galileo_API.Models.ProGrX_Polizas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Galileo_API.Controllers.ProGrX_Polizas
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmPolizasCatEnfermedadesController : ControllerBase
    {
        private readonly FrmPolizasCatEnfermedadesBL _bl;

        public FrmPolizasCatEnfermedadesController(IConfiguration config)
        {
            _bl = new FrmPolizasCatEnfermedadesBL(config);
        }

        [Authorize]
        [HttpGet("Enfermedades_Lista")]
        public ErrorDto<List<EnfermedadVidaDto>> Enfermedades_Lista(int codEmpresa)
            => _bl.Enfermedades_Lista(codEmpresa);

        [Authorize]
        [HttpPost("Enfermedades_Guardar")]
        public ErrorDto<bool> Enfermedades_Guardar(int codEmpresa, [FromBody] EnfermedadVidaSaveParams param)
            => _bl.Enfermedades_Guardar(codEmpresa, param);

        [Authorize]
        [HttpPost("Enfermedades_Eliminar")]
        public ErrorDto<bool> Enfermedades_Eliminar(int codEmpresa, [FromBody] EnfermedadVidaDeleteParams param)
            => _bl.Enfermedades_Eliminar(codEmpresa, param);
    }
}
