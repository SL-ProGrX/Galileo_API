using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [ApiController]
    [Route("api/[controller]")]
    public class FrmCrAnulaAbonosController : ControllerBase
    {
        private readonly FrmCrAnulaAbonosBL _bl;

        public FrmCrAnulaAbonosController(IConfiguration config)
        {
            _bl = new FrmCrAnulaAbonosBL(config);
        }

        [HttpGet("CR_AnulaAbonos_ConsultarOperacion")]
        [Authorize]
        public ErrorDto<CrAnulaAbonosConsultaResponse> CR_AnulaAbonos_ConsultarOperacion(int codEmpresa, int idSolicitud)
        {
            return _bl.CR_AnulaAbonos_ConsultarOperacion(codEmpresa, idSolicitud);
        }

        [HttpPost("CR_AnulaAbonos_CuentaRecomendada")]
        [Authorize]
        public ErrorDto<string> CR_AnulaAbonos_CuentaRecomendada(int codEmpresa, [FromBody] CrAnulaAbonosCuentaRecomendadaRequest request)
        {
            return _bl.CR_AnulaAbonos_CuentaRecomendada(codEmpresa, request);
        }

        [HttpPost("CR_AnulaAbonos_Procesar")]
        [Authorize]
        public ErrorDto<CrAnulaAbonosProcesarResponse> CR_AnulaAbonos_Procesar(int codEmpresa, [FromBody] CrAnulaAbonosProcesarRequest request)
        {
            return _bl.CR_AnulaAbonos_Procesar(codEmpresa, request);
        }
    }
}
