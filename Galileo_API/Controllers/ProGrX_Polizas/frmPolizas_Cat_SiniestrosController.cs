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
    public class FrmPolizasCatSiniestrosController : ControllerBase
    {
        private readonly FrmPolizasCatSiniestrosBL _bl;

        public FrmPolizasCatSiniestrosController(IConfiguration config)
        {
            _bl = new FrmPolizasCatSiniestrosBL(config);
        }

        [Authorize]
        [HttpGet("Siniestros_Lista")]
        public ErrorDto<List<SiniestroTipoDto>> Siniestros_Lista(int codEmpresa)
            => _bl.Siniestros_Lista(codEmpresa);

        [Authorize]
        [HttpGet("Siniestros_Existe")]
        public ErrorDto<SiniestroTipoExisteResult?> Siniestros_Existe(int codEmpresa, [FromQuery] int id)
            => _bl.Siniestros_Existe(codEmpresa, id);

        [Authorize]
        [HttpPost("Siniestros_Guardar")]
        public ErrorDto<bool> Siniestros_Guardar(int codEmpresa, [FromBody] SiniestroTipoSaveParams param)
            => _bl.Siniestros_Guardar(codEmpresa, param);

        [Authorize]
        [HttpPost("Siniestros_Eliminar")]
        public ErrorDto<bool> Siniestros_Eliminar(int codEmpresa, [FromBody] SiniestroTipoDeleteParams param)
            => _bl.Siniestros_Eliminar(codEmpresa, param);
    }
}
