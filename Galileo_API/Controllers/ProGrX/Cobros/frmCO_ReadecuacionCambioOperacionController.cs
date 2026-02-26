using Galileo.Models.ERROR;
using Galileo_API.BusinessTier.ProGrX.Cobros;
using Galileo_API.Models.ProGrX.Cobros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Cobros
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class frmCO_ReadecuacionCambioOperacionController : ControllerBase
    {
        private readonly frmCO_ReadecuacionCambioOperacionBL _bl;

        public frmCO_ReadecuacionCambioOperacionController(frmCO_ReadecuacionCambioOperacionBL bl)
        {
            _bl = bl;
        }
        [Authorize]
        [HttpGet("CO_ReadecuacionCambioOperacion_Obtener")]
        public ErrorDto<CoReadecuacionCambioOperacionObtenerResponse> CO_ReadecuacionCambioOperacion_Obtener([FromQuery] int CodEmpresa, [FromQuery] int idTramite)
        {
            return _bl.CO_ReadecuacionCambioOperacion_Obtener(CodEmpresa, idTramite);
        }
        [Authorize]
        [HttpPost("CO_ReadecuacionCambioOperacion_Aplicar")]
        public ErrorDto<CoReadecuacionCambioOperacionAplicarResponse> CO_ReadecuacionCambioOperacion_Aplicar([FromQuery] int CodEmpresa, [FromBody] CoReadecuacionCambioOperacionAplicarRequest req)
        {
        return  _bl.CO_ReadecuacionCambioOperacion_Aplicar(CodEmpresa, req);
        }    
    }
}