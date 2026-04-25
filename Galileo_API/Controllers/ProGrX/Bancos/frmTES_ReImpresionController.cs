using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo_API.BusinessLogic.ProGrX.Bancos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Bancos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmTesReImpresionController : ControllerBase
    {
        private readonly FrmTesReImpresionBL _ReImpresionBL;

        public FrmTesReImpresionController(IConfiguration config)
        {
            _ReImpresionBL = new FrmTesReImpresionBL(config);
        }
        [HttpGet("TES_ReImpresion_Obtener")]
        public ErrorDto<TesReImpresionModels> TES_ReImpresion_Obtener(int CodEmpresa, int solicitud)
        {
            return _ReImpresionBL.TES_ReImpresion_Obtener(CodEmpresa, solicitud);
        }

        [HttpPost("TES_ReImpresion_Guardar")]
        public ErrorDto<object> TES_ReImpresion_Guardar(int CodEmpresa, TesReImpresionModels solicitud)
        {
            return _ReImpresionBL.TES_ReImpresion_Guardar(CodEmpresa, solicitud);
        }
    }
}
