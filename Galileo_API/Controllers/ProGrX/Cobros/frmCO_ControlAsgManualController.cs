using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX_Cobros;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Cobros;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCOControlAsgManualController : ControllerBase
    {
        private readonly FrmCOControlAsgManualBL _bl;

        public FrmCOControlAsgManualController(IConfiguration config)
        {
            _bl = new FrmCOControlAsgManualBL(config);
        }

        [Authorize]
        [HttpGet("Co_ControlAsgManual_Expedientes_Obtener")]
        public ErrorDto<List<CoControlAsgManualExpedienteItem>> Co_ControlAsgManual_Expedientes_Obtener(int CodEmpresa,int soloSinAsignar,int soloMorosos)
        {
            return _bl.Co_ControlAsgManual_Expedientes_Obtener(CodEmpresa, soloSinAsignar, soloMorosos);
        }
        [Authorize]
        [HttpGet("Co_ControlAsgManual_Expediente_Detalle_Obtener")]
        public ErrorDto<CoControlAsgManualExpedienteDetalle> Co_ControlAsgManual_Expediente_Detalle_Obtener(
            int CodEmpresa,
            string cedula)
        {
            return _bl.Co_ControlAsgManual_Expediente_Detalle_Obtener(CodEmpresa, cedula);
        }

        [Authorize]
        [HttpGet("Co_ControlAsgManual_Usuarios_Obtener")]
        public ErrorDto<List<CoControlAsgManualUsuarioItem>> Co_ControlAsgManual_Usuarios_Obtener(int CodEmpresa)
        {
            return _bl.Co_ControlAsgManual_Usuarios_Obtener(CodEmpresa);
        }
        [Authorize]
        [HttpPost("Co_ControlAsgManual_Aplicar")]
        public ErrorDto Co_ControlAsgManual_Aplicar(
            int CodEmpresa,
            string usuario,
            CoControlAsgManualAplicarRequest data)
        {
            return _bl.Co_ControlAsgManual_Aplicar(CodEmpresa, usuario, data);
        }
    }
}
