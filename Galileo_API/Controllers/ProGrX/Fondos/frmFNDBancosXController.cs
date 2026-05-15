using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Fondos;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.Controllers.ProGrX.Fondos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmFndBancosXController : ControllerBase
    {
        private readonly FrmFndBancosXBl _bl;

        public FrmFndBancosXController(IConfiguration config)
        {
            _bl = new FrmFndBancosXBl(config);
        }

        [Authorize]
        [HttpGet("BancosX_Obtener")]
        public ErrorDto<List<FndBancosXModel>> BancosX_Obtener(int codEmpresa)
        {
            return _bl.BancosX_Obtener(codEmpresa);
        }

        [Authorize]
        [HttpPost("BancosX_Insertar")]
        public ErrorDto BancosX_Insertar(int codEmpresa)
        {
            return _bl.BancosX_Insertar(codEmpresa);
        }

        [Authorize]
        [HttpPut("BancosX_Actualizar")]
        public ErrorDto BancosX_Actualizar(int codEmpresa, [FromBody] FndBancosXUpdateParam param)
        {
            return _bl.BancosX_Actualizar(codEmpresa, param);
        }
    }
}