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
    public class FrmCntXUtilEliminaContaController : ControllerBase
    {
        private readonly FrmCntXUtilEliminaContaBl _bl;

        public FrmCntXUtilEliminaContaController(IConfiguration config)
            => _bl = new FrmCntXUtilEliminaContaBl(config);

        [HttpGet("CntxUtil_Contabilidades_Obtener")]
        public ErrorDto<List<CntxContabilidadListaDto>> CntxUtil_Contabilidades_Obtener(int codEmpresa)
        {
            return _bl.CntxUtil_Contabilidades_Obtener(codEmpresa);
        }

        [HttpPost("CntxUtil_Contabilidades_Eliminar")]
        public ErrorDto<bool> CntxUtil_Contabilidades_Eliminar([FromBody] CntxUtilEliminaContabilidadesRequestDto request)
        {
            return _bl.CntxUtil_Contabilidades_Eliminar(request);
        }
    }
}