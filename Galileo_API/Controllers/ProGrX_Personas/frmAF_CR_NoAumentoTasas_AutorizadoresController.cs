using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Personas;
using Galileo_API.BusinessLogic.ProGrX_Personas;

namespace Galileo_API.Controllers.ProGrX_Personas
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAFCrNoAumentoTasasAutorizadoresController : ControllerBase
    {
        private readonly FrmAFCrNoAumentoTasasAutorizadoresBL _bl;

        public FrmAFCrNoAumentoTasasAutorizadoresController(IConfiguration config)
        {
            _bl = new FrmAFCrNoAumentoTasasAutorizadoresBL(config);
        }

        [Authorize]
        [HttpGet("AF_NAT_Autorizadores_Obtener")]
        public ErrorDto<List<AfNatAutorizadores>> AF_NAT_Autorizadores_Obtener(int CodEmpresa)
        {
            return _bl.AF_NAT_Autorizadores_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpPost("AF_NAT_Autorizadores_Asignar")]
        public ErrorDto AF_NAT_Autorizadores_Asignar(int CodEmpresa, string A_Usuario, string Mov, string usuario)
        {
            return _bl.AF_NAT_Autorizadores_Asignar(CodEmpresa, A_Usuario, Mov, usuario);
        }
        
    }
}
