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
    public class FrmCrAutorizacionTranferenciasController : ControllerBase
    {
        private readonly FrmCrAutorizacionTranferenciasBL _bl;

        public FrmCrAutorizacionTranferenciasController(IConfiguration config)
        {
            _bl = new FrmCrAutorizacionTranferenciasBL(config);
        }

        [Authorize]
        [HttpGet("CrAutorizacionTranferencias_Tags_Obtener")]
        public ErrorDto<List<CrAutorizacionTranferenciasTag>> CrAutorizacionTranferencias_Tags_Obtener(int CodEmpresa, string Usuario)
        {
            return _bl.CrAutorizacionTranferencias_Tags_Obtener(CodEmpresa, Usuario);
        }

        [Authorize]
        [HttpGet("CrAutorizacionTranferencias_Solicitudes_Obtener")]
        public ErrorDto<List<CrAutorizacionTranferenciasSolicitud>> CrAutorizacionTranferencias_Solicitudes_Obtener(
            int CodEmpresa,
            DateTime FechaDtpFInicio,
            string CodigoEtiqueta)
        {
            return _bl.CrAutorizacionTranferencias_Solicitudes_Obtener(CodEmpresa, FechaDtpFInicio, CodigoEtiqueta);
        }
    }
}
