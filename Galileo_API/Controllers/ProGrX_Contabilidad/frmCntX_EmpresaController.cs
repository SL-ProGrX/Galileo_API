using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Contabilidad
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCntXEmpresaController : ControllerBase
    {
        private readonly FrmCntXEmpresaBl _bl;

        public FrmCntXEmpresaController(IConfiguration config) => _bl = new FrmCntXEmpresaBl(config);

        [HttpGet("CntXEmpresa_Obtener")]
        public ErrorDto<CntXEmpresaDto> CntXEmpresa_Obtener(int codEmpresa)
        {
            return _bl.CntXEmpresa_Obtener(codEmpresa);
        }

        [HttpPost("CntXEmpresa_Guardar")]
        public ErrorDto CntXEmpresa_Guardar(int codEmpresa, string usuario, CntXEmpresaDto request)
        {
            return _bl.CntXEmpresa_Guardar(codEmpresa, usuario, request);
        }
    }
}