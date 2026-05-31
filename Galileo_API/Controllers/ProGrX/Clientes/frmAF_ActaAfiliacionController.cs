using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Clientes;
using Galileo.Models.ERROR;

namespace Galileo.Controllers.ProGrX.Clientes
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAfActaAfiliacionController : ControllerBase
    {
        private readonly FrmAfActaAfiliacionBL _bl;

        public FrmAfActaAfiliacionController(IConfiguration config)
        {
            _bl = new FrmAfActaAfiliacionBL(config);
        }

        [Authorize]
        [HttpGet("AF_ActaAfiliacio_Obtener")]
        public ErrorDto<long> AF_ActaAfiliacio_Obtener(int CodEmpresa, string usuario)
        {
            return _bl.AF_ActaAfiliacio_Obtener(CodEmpresa, usuario);
        }
    }
}