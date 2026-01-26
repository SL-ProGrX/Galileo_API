using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo_API.BusinessLogic.ProGrX.Bancos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PgxAPI.Controllers.ProGrX.Bancos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmTesReposicionController : ControllerBase
    {
        private readonly FrmTesReposicionBL _reposicionBL;

        public FrmTesReposicionController(IConfiguration config)
        {
            _reposicionBL = new FrmTesReposicionBL(config);
        }

        [HttpGet("TES_Reposicion_Obtener")]
        public ErrorDto<TesReposicionData> TES_Reposicion_Obtener(int CodEmpresa, int solicitud)
        {
            return _reposicionBL.TES_Reposicion_Obtener(CodEmpresa, solicitud);
        }

        [HttpPost("TES_Reposicion_Guardar")]
        public ErrorDto TES_Reposicion_Guardar(int CodEmpresa, TesReposicionData solicitud)
        {
            return _reposicionBL.TES_Reposicion_Guardar(CodEmpresa, solicitud);
        }
    }
}
