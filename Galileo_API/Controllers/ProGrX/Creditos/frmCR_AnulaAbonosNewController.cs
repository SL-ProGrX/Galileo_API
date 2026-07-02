using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCrAnulaAbonosNewController : ControllerBase
    {
        private readonly FrmCrAnulaAbonosNewBl _bl;

        public FrmCrAnulaAbonosNewController(IConfiguration config)
            => _bl = new FrmCrAnulaAbonosNewBl(config);

        [HttpGet("CrAnulaAbonosNew_Operacion_Obtener")]
        public ErrorDto<CrAnulaAbonosNewConsultaData> CrAnulaAbonosNew_Operacion_Obtener(
            int codEmpresa,
            int operacion)
        {
            return _bl.CrAnulaAbonosNew_Operacion_Obtener(codEmpresa, operacion);
        }

        [HttpGet("CrAnulaAbonosNew_CuentaRecomendada_Obtener")]
        public ErrorDto<string> CrAnulaAbonosNew_CuentaRecomendada_Obtener(
            int codEmpresa,
            int operacion,
            decimal amortizacion)
        {
            return _bl.CrAnulaAbonosNew_CuentaRecomendada_Obtener(
                codEmpresa,
                new CrAnulaAbonosNewCuentaRecomendadaRequest
                {
                    operacion = operacion,
                    amortizacion = amortizacion
                });
        }

        [HttpPost("CrAnulaAbonosNew_Aplicar")]
        public ErrorDto<CrAnulaAbonosNewAplicarResultadoData> CrAnulaAbonosNew_Aplicar(
            int codEmpresa,
            CrAnulaAbonosNewAplicarRequest request)
        {
            return _bl.CrAnulaAbonosNew_Aplicar(codEmpresa, request);
        }
    }
}