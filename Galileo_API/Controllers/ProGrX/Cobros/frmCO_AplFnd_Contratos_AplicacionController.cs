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
    public class FrmCoAplFndContratosAplicacionController : ControllerBase
    {
        private readonly FrmCoAplFndContratosAplicacionBl _bl;

        public FrmCoAplFndContratosAplicacionController(IConfiguration config) => _bl = new FrmCoAplFndContratosAplicacionBl(config);

        
        [HttpGet("CO_AplFndContrApl_Informacion_Obtener")]
        public ErrorDto<List<CoAplFndContrAplInformacionData>> CO_AplFndContrApl_Informacion_Obtener(int codEmpresa)
        {
            return _bl.CO_AplFndContrApl_Informacion_Obtener(codEmpresa);
        }

        [HttpPost("CO_AplFndContrApl_Aplicar")]
        public ErrorDto<CoAplFndContrAplicadosResult> CO_AplFndContrApl_Aplicar(int CodEmpresa, ContratosAplicarRequest request)
        {
            return _bl.CO_AplFndContrApl_Aplicar(CodEmpresa, request);
        }
    }
}


