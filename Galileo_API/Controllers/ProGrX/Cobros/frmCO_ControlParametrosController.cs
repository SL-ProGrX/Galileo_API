using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cobros;
using Galileo_API.BusinessLogicTier.ProGrX.Cobros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Cobros
{
    [ApiController]
    [Route("api/[controller]")]
    public class FrmCOControlParametrosController : ControllerBase
    {
        private readonly FrmCOControlParametrosBL _bl;

        public FrmCOControlParametrosController(IConfiguration config)
        {
            _bl = new FrmCOControlParametrosBL(config);
        }

        [Authorize]
        [HttpGet("Co_ControlParametros_Lista_Obtener")]
        public ErrorDto<CoControlParametrosListaResult> Co_ControlParametros_Lista_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.Co_ControlParametros_Lista_Obtener(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpGet("Co_ControlParametros_Lista_Export")]
        public ErrorDto<CoControlParametrosListaResult> Co_ControlParametros_Lista_Export(int CodEmpresa, string filtros)
        {
            return _bl.Co_ControlParametros_Lista_Export(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpPost("Co_ControlParametros_Guardar")]
        public ErrorDto Co_ControlParametros_Guardar(int CodEmpresa, [FromBody] CoControlParametrosGuardarRequest req)
        {
            return _bl.Co_ControlParametros_Guardar(CodEmpresa, req);
        }
    }
}
