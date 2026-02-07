using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Cobros;
using Galileo_API.Models.ProGrX.Cobros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Cobros
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCoAplExcContratosAplicacionController : ControllerBase
    {
        private readonly FrmCoAplExcContratosAplicacionBl _bl;

        public FrmCoAplExcContratosAplicacionController(IConfiguration config) => _bl = new FrmCoAplExcContratosAplicacionBl(config);

        
        [HttpGet("CO_AplExcContrApl_Informacion_Obtener")]
        public ErrorDto<List<CoAplExcContrAplInformacionData>> CO_AplExcContrApl_Informacion_Obtener(int codEmpresa)
        {
            return _bl.CO_AplExcContrApl_Informacion_Obtener(codEmpresa);
        }

        [HttpPost("CO_AplExcContrApl_Aplicar")]
        public ErrorDto<CoAplExcContrAplicadosResult> CO_AplExcContrApl_Aplicar(int CodEmpresa, ExcContratosAplicarRequest request)
        {
            return _bl.CO_AplExcContrApl_Aplicar(CodEmpresa, request);
        }
    }
}


