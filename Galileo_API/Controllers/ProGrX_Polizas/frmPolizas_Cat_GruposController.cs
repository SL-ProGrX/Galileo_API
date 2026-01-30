using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Polizas;
using Galileo_API.DataBaseTier.ProGrX_Polizas;
using Galileo_API.Models.ProGrX_Polizas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Galileo_API.Controllers.ProGrX_Polizas
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmPolizasCatGruposController : ControllerBase
    {
        private readonly FrmPolizasCatGruposBL _bl;

        public FrmPolizasCatGruposController(IConfiguration config)
        {
            _bl = new FrmPolizasCatGruposBL(config);
        }

        [Authorize]
        [HttpGet("PolizaGrupos_Lista")]
        public ErrorDto<List<PolizaGrupoDto>> PolizaGrupos_Lista(int codEmpresa)
            => _bl.PolizaGrupos_Lista(codEmpresa);

        [Authorize]
        [HttpGet("PolizaGrupos_Existe")]
        public ErrorDto<PolizaGrupoExisteResult?> PolizaGrupos_Existe(int codEmpresa, [FromQuery] int id)
            => _bl.PolizaGrupos_Existe(codEmpresa, id);

        [Authorize]
        [HttpPost("PolizaGrupos_Guardar")]
        public ErrorDto<bool> PolizaGrupos_Guardar(int codEmpresa, [FromBody] PolizaGrupoSaveParams param)
            => _bl.PolizaGrupos_Guardar(codEmpresa, param);

        [Authorize]
        [HttpPost("PolizaGrupos_Eliminar")]
        public ErrorDto<bool> PolizaGrupos_Eliminar(int codEmpresa, [FromBody] PolizaGrupoDeleteParams param)
            => _bl.PolizaGrupos_Eliminar(codEmpresa, param);
    }
}
