using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Cobros;
using Galileo_API.Models.ProGrX.Cobros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Galileo_API.Controllers.ProGrX.Cobros
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCoControlAsgAutoController : ControllerBase
    {
        private readonly FrmCoControlAsgAutoBL _bl;

        public FrmCoControlAsgAutoController(IConfiguration config)
        {
            _bl = new FrmCoControlAsgAutoBL(config);
        }

        [Authorize]
        [HttpGet("CbrUsuarios_Activos_Lista")]
        public ErrorDto<List<CbrUsuarioResult>> CbrUsuarios_Activos_Lista(int codEmpresa)
        {
            return _bl.CbrUsuarios_Activos_Lista(codEmpresa);
        }

        [Authorize]
        [HttpPost("CbrUsuarios_Grupos_List")]
        public ErrorDto<List<CbrUsuarioGrupoListResult>> CbrUsuarios_Grupos_List(int codEmpresa, [FromBody] CbrUsuarioGrupoListParams param)
        {
            return _bl.CbrUsuarios_Grupos_List(codEmpresa, param);
        }

        [Authorize]
        [HttpPost("CbrControlDistribucion")]
        public ErrorDto<CbrControlDistribucionResult?> CbrControlDistribucion(int codEmpresa, [FromBody] CbrControlDistribucionParams param)
        {
            return _bl.CbrControlDistribucion(codEmpresa, param);
        }
    }
}
