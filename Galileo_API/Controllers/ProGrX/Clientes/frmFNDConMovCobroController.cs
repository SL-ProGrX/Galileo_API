using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Clientes;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.Controllers.ProGrX.Clientes
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmFndConMovCobroController : ControllerBase
    {
        private readonly FrmFndConMovCobroBl _bl;

        public FrmFndConMovCobroController(IConfiguration config)
        {
            _bl = new FrmFndConMovCobroBl(config);
        }

        [Authorize]
        [HttpGet("Fnd_Planes_Lista_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_Planes_Lista_Obtener(int CodEmpresa)
        {
            return _bl.Fnd_Planes_Lista_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpPost("Fnd_ConMovCobro_Obtener")]
        public ErrorDto<List<FndConMovCobroResult>> Fnd_ConMovCobro_Obtener(
            int CodEmpresa, [FromBody] FndConMovCobroRequest request)
        {
            return _bl.Fnd_ConMovCobro_Obtener(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("Fnd_ConMovCobro_SinContrato_Obtener")]
        public ErrorDto<List<FndConMovCobroResult>> Fnd_ConMovCobro_SinContrato_Obtener(
            int CodEmpresa, [FromBody] FndConMovCobroRequest request)
        {
            return _bl.Fnd_ConMovCobro_SinContrato_Obtener(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("Fnd_AcreditaMovCbrPendiente")]
        public ErrorDto<bool> Fnd_AcreditaMovCbrPendiente(
            int CodEmpresa, [FromBody] FndAcreditaMovCbrPendienteRequest request)
        {
            return _bl.Fnd_AcreditaMovCbrPendiente(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("Fnd_ConMovCobro_EntradaPlanilla_Obtener")]
        public ErrorDto<FndConMovCobroResumenResult?> Fnd_ConMovCobro_EntradaPlanilla_Obtener(
            int CodEmpresa, [FromBody] FndConMovCobroResumenRequest request)
        {
            return _bl.Fnd_ConMovCobro_EntradaPlanilla_Obtener(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("Fnd_ConMovCobro_PlanillaRegistrada_Obtener")]
        public ErrorDto<FndConMovCobroResumenResult?> Fnd_ConMovCobro_PlanillaRegistrada_Obtener(
            int CodEmpresa, [FromBody] FndConMovCobroResumenRequest request)
        {
            return _bl.Fnd_ConMovCobro_PlanillaRegistrada_Obtener(CodEmpresa, request);
        }
    }
}