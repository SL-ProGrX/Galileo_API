using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Nucleo;
using Galileo_API.Models.ProGrX_Nucleo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Nucleo
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmSysAplIntegradorController : ControllerBase
    {
        private readonly FrmSysAplIntegradorBL _bl;

        public FrmSysAplIntegradorController(IConfiguration config)
        {
            _bl = new FrmSysAplIntegradorBL(config);
        }

        [Authorize]
        [HttpGet("SYS_APL_Integrador_Lineas_Lista")]
        public ErrorDto<List<AplLineaDto>> SYS_APL_Integrador_Lineas_Lista(int codEmpresa)
        {
            return _bl.SYS_APL_Integrador_Lineas_Lista(codEmpresa);
        }

        [Authorize]
        [HttpGet("SYS_APL_Integrador_Tramite_Lista")]
        public ErrorDto<List<AplTramiteDto>> SYS_APL_Integrador_Tramite_Lista(int codEmpresa, [FromQuery] AplListaRequest request)
        {
            return _bl.SYS_APL_Integrador_Tramite_Lista(codEmpresa, request);
        }

        [Authorize]
        [HttpGet("SYS_APL_Integrador_Solicitud_Lista")]
        public ErrorDto<List<AplSolicitudDto>> SYS_APL_Integrador_Solicitud_Lista(int codEmpresa, [FromQuery] AplListaRequest request)
        {
            return _bl.SYS_APL_Integrador_Solicitud_Lista(codEmpresa, request);
        }

        [Authorize]
        [HttpPost("SYS_APL_Integrador_Tramite_Atiende")]
        public ErrorDto SYS_APL_Integrador_Tramite_Atiende(int codEmpresa, long apl_operacion, string usuario)
        {
            return _bl.SYS_APL_Integrador_Tramite_Atiende(codEmpresa, apl_operacion, usuario);
        }

        [Authorize]
        [HttpPost("SYS_APL_Integrador_Solicitud_Atiende")]
        public ErrorDto SYS_APL_Integrador_Solicitud_Atiende(int codEmpresa, long num_solicitud, string usuario)
        {
            return _bl.SYS_APL_Integrador_Solicitud_Atiende(codEmpresa, num_solicitud, usuario);
        }

        [Authorize]
        [HttpGet("SYS_APL_Integrador_Monitor")]
        public ErrorDto<AplMonitorDto?> SYS_APL_Integrador_Monitor(int codEmpresa)
        {
            return _bl.SYS_APL_Integrador_Monitor(codEmpresa);
        }
    }
}
