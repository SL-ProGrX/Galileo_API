using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Conciliacion;
using Galileo_API.Models.ProGrX_Conciliacion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Conciliacion
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmVerificaSaldosAseController : ControllerBase
    {
        private readonly FrmVerificaSaldosAsebl _bl;

        public FrmVerificaSaldosAseController(IConfiguration config)
        {
            _bl = new FrmVerificaSaldosAsebl(config);
        }

        [Authorize]
        [HttpGet("ASE_VerificaSaldos_Inicial_Obtener")]
        public ErrorDto<AseVerificaSaldosInicialData> ASE_VerificaSaldos_Inicial_Obtener(int CodEmpresa)
        {
            return _bl.ASE_VerificaSaldos_Inicial_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("ASE_VerificaSaldos_Periodos_Dropdown_Obtener")]
        public ErrorDto<List<AseVerificaSaldosPeriodoData>> ASE_VerificaSaldos_Periodos_Dropdown_Obtener(int CodEmpresa)
        {
            return _bl.ASE_VerificaSaldos_Periodos_Dropdown_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpPost("ASE_VerificaSaldos_Lista_Obtener")]
        public ErrorDto<AseVerificaSaldosListaResult> ASE_VerificaSaldos_Lista_Obtener(int CodEmpresa, [FromBody] AseVerificaSaldosListaRequest? request)
        {
            return _bl.ASE_VerificaSaldos_Lista_Obtener(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("ASE_VerificaSaldos_Lista_Export")]
        public ErrorDto<AseVerificaSaldosListaResult> ASE_VerificaSaldos_Lista_Export(int CodEmpresa, [FromBody] AseVerificaSaldosListaRequest? request)
        {
            return _bl.ASE_VerificaSaldos_Lista_Export(CodEmpresa, request);
        }
    }
}