namespace Galileo_API.Controllers.ProGrX.Creditos
{
    using Galileo.Models.ERROR;
    using Galileo_API.BusinessLogic.ProGrX.Creditos;
    using Galileo_API.Models.ProGrX.Creditos;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCrComitesParametrosController : ControllerBase
    {
        private readonly FrmCrComitesParametrosBL _bl;

        public FrmCrComitesParametrosController(IConfiguration config)
        {
            _bl = new FrmCrComitesParametrosBL(config);
        }

        [Authorize]
        [HttpGet("CrComitesParametros_Obtener")]
        public ErrorDto<List<CrComitesParametroModel>> CrComitesParametros_Obtener(int CodEmpresa)
        {
            return _bl.CrComitesParametros_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpPost("CrComitesParametros_Actualizar")]
        public ErrorDto CrComitesParametros_Actualizar(int CodEmpresa, CrComitesParametroActualizarRequest request)
        {
            return _bl.CrComitesParametros_Actualizar(CodEmpresa, request);
        }
    }
}
