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
    public class FrmPolizasCatIncendioCausasController : ControllerBase
    {
        private readonly FrmPolizasCatIncendioCausasBL _bl;

        public FrmPolizasCatIncendioCausasController(IConfiguration config)
        {
            _bl = new FrmPolizasCatIncendioCausasBL(config);
        }

        [Authorize]
        [HttpGet("IncendioCausas_Lista")]
        public ErrorDto<List<IncendioCausaDto>> IncendioCausas_Lista(int codEmpresa)
            => _bl.IncendioCausas_Lista(codEmpresa);

        [Authorize]
        [HttpPost("IncendioCausas_Insertar")]
        public ErrorDto<bool> IncendioCausas_Insertar(int codEmpresa, [FromBody] IncendioCausaSaveParams param)
            => _bl.IncendioCausas_Insertar(codEmpresa, param);

        [Authorize]
        [HttpPost("IncendioCausas_Actualizar")]
        public ErrorDto<bool> IncendioCausas_Actualizar(int codEmpresa, [FromBody] IncendioCausaUpdateParams param)
            => _bl.IncendioCausas_Actualizar(codEmpresa, param);

        [Authorize]
        [HttpPost("IncendioCausas_Eliminar")]
        public ErrorDto<bool> IncendioCausas_Eliminar(int codEmpresa, [FromBody] IncendioCausaDeleteParams param)
            => _bl.IncendioCausas_Eliminar(codEmpresa, param);
    }
}
