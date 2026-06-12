using Galileo.Models.AH;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Patrimonio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Patrimonio
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmAhExcedentesCapIndController : ControllerBase
    {
        private readonly FrmAhExcedentesCapIndBL _bl;

        public FrmAhExcedentesCapIndController(IConfiguration config)
        {
            _bl = new FrmAhExcedentesCapIndBL(config);
        }

        [HttpPost("AH_ExcedentesCapInd_Cargar")]
        public ErrorDto<FrmAhExcedentesCapIndCargarResponse> AH_ExcedentesCapInd_Cargar(
            [FromQuery] int codEmpresa,
            [FromBody] FrmAhExcedentesCapIndListaRequest? request)
        {
            return _bl.AH_ExcedentesCapInd_Cargar(codEmpresa, request);
        }

        [HttpPost("AH_ExcedentesCapInd_Capitalizaciones_Lista")]
        public ErrorDto<List<FrmAhExcedentesCapIndListadoDto>> AH_ExcedentesCapInd_Capitalizaciones_Lista(
            [FromQuery] int codEmpresa,
            [FromBody] FrmAhExcedentesCapIndListaRequest? request)
        {
            return _bl.AH_ExcedentesCapInd_Capitalizaciones_Lista(codEmpresa, request);
        }

        [HttpGet("AH_ExcedentesCapInd_Cedula_Consultar")]
        public ErrorDto<FrmAhExcedentesCapIndCedulaDto> AH_ExcedentesCapInd_Cedula_Consultar(
            [FromQuery] int codEmpresa,
            [FromQuery] string cedula)
        {
            return _bl.AH_ExcedentesCapInd_Cedula_Consultar(codEmpresa, cedula);
        }

        [HttpPost("AH_ExcedentesCapInd_Guardar")]
        public ErrorDto<FrmAhExcedentesCapIndProcesoResponse> AH_ExcedentesCapInd_Guardar(
            [FromQuery] int codEmpresa,
            [FromBody] FrmAhExcedentesCapIndGuardarRequest? request)
        {
            return _bl.AH_ExcedentesCapInd_Guardar(codEmpresa, request);
        }

        [HttpDelete("AH_ExcedentesCapInd_Eliminar")]
        public ErrorDto<FrmAhExcedentesCapIndProcesoResponse> AH_ExcedentesCapInd_Eliminar(
            [FromQuery] int codEmpresa,
            [FromQuery] int excCapInd,
            [FromQuery] string usuario)
        {
            return _bl.AH_ExcedentesCapInd_Eliminar(codEmpresa, excCapInd, usuario);
        }
    }
}
