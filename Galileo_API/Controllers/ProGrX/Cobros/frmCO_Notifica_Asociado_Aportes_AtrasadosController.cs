using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Cobros;
using Galileo_API.Models.ProGrX.Cobros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Cobros
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCONotificaAsociadoAportesAtrasadosController : ControllerBase
    {
        private readonly FrmCONotificaAsociadoAportesAtrasadosBL BL;

        public FrmCONotificaAsociadoAportesAtrasadosController(IConfiguration config)
        {
            BL = new FrmCONotificaAsociadoAportesAtrasadosBL(config);
        }

        [Authorize]
        [HttpGet("CO_Notifica_Asociado_Aportes_Atrasados_Lista_Obtener")]
        public ErrorDto<CoNotificaAsociadoAportesAtrasadosListaResult> CO_Notifica_Asociado_Aportes_Atrasados_Lista_Obtener(
            int CodEmpresa,
            string? cedula)
        {
            return BL.CO_Notifica_Asociado_Aportes_Atrasados_Lista_Obtener(CodEmpresa, cedula);
        }

        [Authorize]
        [HttpGet("CO_Notifica_Asociado_Aportes_Atrasados_Lista_Export")]
        public ErrorDto<CoNotificaAsociadoAportesAtrasadosListaResult> CO_Notifica_Asociado_Aportes_Atrasados_Lista_Export(
            int CodEmpresa,
            string? cedula)
        {
            return BL.CO_Notifica_Asociado_Aportes_Atrasados_Lista_Export(CodEmpresa, cedula);
        }

        [Authorize]
        [HttpPost("CO_Notifica_Asociado_Aportes_Atrasados_Enviar")]
        public ErrorDto CO_Notifica_Asociado_Aportes_Atrasados_Enviar(int CodEmpresa,[FromBody] CoNotificaAsociadoAportesAtrasadosEnviarRequest? req)
        {
            if (req == null)
            {
                return DbHelper.ErrorResponse("No se recibió la solicitud.", -2);
            }

            return BL.CO_Notifica_Asociado_Aportes_Atrasados_Enviar(CodEmpresa, req);
        }
    }
}