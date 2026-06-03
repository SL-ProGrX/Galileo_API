using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models.ERROR;
using Galileo.BusinessLogic.ProGrX_Personas;
using Galileo.Models.ProGrX_Personas;

namespace Galileo.Controllers.ProGrX_Personas
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAFCrNoAumentoTasasAutorizacionController : ControllerBase
    {
        private readonly FrmAFCrNoAumentoTasasAutorizacionBL _bl;

        public FrmAFCrNoAumentoTasasAutorizacionController(IConfiguration config)
        {
            _bl = new FrmAFCrNoAumentoTasasAutorizacionBL(config);
        }

        [Authorize]
        [HttpPost("AF_NAT_Autorizacion_Obtener")]
        public ErrorDto<List<AfNatAutorizacion>> AF_NAT_Autorizacion_Obtener(int CodEmpresa, [FromBody] AfNatAutorizacionFiltros Filtro)
        {
            return _bl.AF_NAT_Autorizacion_Obtener(CodEmpresa, Filtro);
        }

        [Authorize]
        [HttpPost("AF_NAT_Autorizacion_Autorizar")]
        public ErrorDto AF_NAT_Autorizacion_Autorizar(int CodEmpresa, int RenunciaId, string usuario)
        {
            return _bl.AF_NAT_Autorizacion_Autorizar(CodEmpresa, RenunciaId, usuario);
        }
    }
}
