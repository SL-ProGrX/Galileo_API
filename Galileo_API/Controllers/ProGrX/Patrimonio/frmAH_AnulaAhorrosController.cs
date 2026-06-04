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
    public class FrmAHAnulaAhorrosController : ControllerBase
    {
        private readonly FrmAHAnulaAhorrosBL _bl;

        public FrmAHAnulaAhorrosController(IConfiguration config)
        {
            _bl = new FrmAHAnulaAhorrosBL(config);
        }

        [HttpGet("Ah_AnulaAhorros_Consulta_Obtener")]
        public ErrorDto<FrmAhAnulaAhorrosConsultaResponse?> Ah_AnulaAhorros_Consulta_Obtener(int CodEmpresa, string Cedula)
            => _bl.Ah_AnulaAhorros_Consulta_Obtener(CodEmpresa, Cedula);

        [HttpGet("Ah_AnulaAhorros_Movimientos_Obtener")]
        public ErrorDto<List<FrmAhAnulaAhorrosMovimientoResponse>> Ah_AnulaAhorros_Movimientos_Obtener(int CodEmpresa, string Cedula, string TipoRubro)
            => _bl.Ah_AnulaAhorros_Movimientos_Obtener(CodEmpresa, Cedula, TipoRubro);

        [HttpPost("Ah_AnulaAhorros_Procesar")]
        public ErrorDto<FrmAhAnulaAhorrosProcesarResponse> Ah_AnulaAhorros_Procesar(int CodEmpresa, FrmAhAnulaAhorrosProcesarRequest request)
            => _bl.Ah_AnulaAhorros_Procesar(CodEmpresa, request);
    }
}
