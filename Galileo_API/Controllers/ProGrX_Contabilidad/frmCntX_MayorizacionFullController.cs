using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Contabilidad
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCntXMayorizacionFullController : ControllerBase
    {
        private readonly FrmCntXMayorizacionFullBl _bl;

        public FrmCntXMayorizacionFullController(IConfiguration config)
        {
            _bl = new FrmCntXMayorizacionFullBl(config);
        }

        [Authorize]
        [HttpGet("CntX_TiposAsientos_Listar")]
        public ErrorDto<List<CntxTipoAsientoDto>> CntX_TiposAsientos_Listar(int codEmpresa,int codContabilidad)
        {
            return _bl.CntX_TiposAsientos_Listar(codEmpresa, codContabilidad);
        }

        [Authorize]
        [HttpPost("Procesar")]
        public ErrorDto<bool> Procesar(int codEmpresa,int codContabilidad,CntxMayorizacionProcesarDto request)
        {
            return _bl.Procesar(codEmpresa, codContabilidad, request);
        }
    }
}
