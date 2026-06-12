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
    public class FrmAhExcedentesAjusteController : ControllerBase
    {
        private readonly FrmAhExcedentesAjusteBL _bl;

        public FrmAhExcedentesAjusteController(IConfiguration config)
        {
            _bl = new FrmAhExcedentesAjusteBL(config);
        }

        [HttpPost("AH_ExcedentesAjuste_Cargar")]
        public ErrorDto<FrmAhExcedentesAjusteCargarResponse> AH_ExcedentesAjuste_Cargar(
            [FromQuery] int codEmpresa,
            [FromBody] FrmAhExcedentesAjustePendienteListaRequest? request)
        {
            return _bl.AH_ExcedentesAjuste_Cargar(codEmpresa, request);
        }

        [HttpGet("AH_ExcedentesAjuste_Periodos_Lista")]
        public ErrorDto<List<ExcPeriodosDto>> AH_ExcedentesAjuste_Periodos_Lista(
            [FromQuery] int codEmpresa)
        {
            return _bl.AH_ExcedentesAjuste_Periodos_Lista(codEmpresa);
        }

        [HttpPost("AH_ExcedentesAjuste_Pendientes_Lista")]
        public ErrorDto<List<FrmAhExcedentesAjustePendienteDto>> AH_ExcedentesAjuste_Pendientes_Lista(
            [FromQuery] int codEmpresa,
            [FromBody] FrmAhExcedentesAjustePendienteListaRequest? request)
        {
            return _bl.AH_ExcedentesAjuste_Pendientes_Lista(codEmpresa, request);
        }

        [HttpGet("AH_ExcedentesAjuste_Cedula_Consultar")]
        public ErrorDto<FrmAhExcedentesAjusteCedulaDto> AH_ExcedentesAjuste_Cedula_Consultar(
            [FromQuery] int codEmpresa,
            [FromQuery] string cedula)
        {
            return _bl.AH_ExcedentesAjuste_Cedula_Consultar(codEmpresa, cedula);
        }

        [HttpPost("AH_ExcedentesAjuste_Guardar")]
        public ErrorDto<FrmAhExcedentesAjusteProcesoResponse> AH_ExcedentesAjuste_Guardar(
            [FromQuery] int codEmpresa,
            [FromBody] FrmAhExcedentesAjusteGuardarRequest? request)
        {
            return _bl.AH_ExcedentesAjuste_Guardar(codEmpresa, request);
        }

        [HttpDelete("AH_ExcedentesAjuste_Eliminar")]
        public ErrorDto<FrmAhExcedentesAjusteProcesoResponse> AH_ExcedentesAjuste_Eliminar(
            [FromQuery] int codEmpresa,
            [FromQuery] int ajusteId,
            [FromQuery] string usuario)
        {
            return _bl.AH_ExcedentesAjuste_Eliminar(codEmpresa, ajusteId, usuario);
        }
    }
}
