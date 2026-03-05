using Galileo.Models.ERROR;
using Galileo_API.BusinessTier.ProGrX.Cobros;
using Galileo_API.Models.ProGrX.Cobros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Cobros
{
    [ApiController]
    [Route("api/[controller]")]
    public class FrmCOReadecuacionCambioOperacionController : ControllerBase
    {
        private readonly FrmCOReadecuacionCambioOperacionBL _bl;

        public FrmCOReadecuacionCambioOperacionController(IConfiguration config)
        {
            _bl = new FrmCOReadecuacionCambioOperacionBL(config);
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
        [Authorize]
        [HttpGet("CO_Readecuacion_ReporteOperacionNueva_Obtener")]
        public ErrorDto<CoReadecuacionReporteOperacionNuevaDto> CO_Readecuacion_ReporteOperacionNueva_Obtener(
            [FromQuery] int CodEmpresa,
            [FromQuery] long id_solicitud)
        {
            var req = new CoReadecuacionReporteOperacionNuevaRequest
            {
                id_solicitud = id_solicitud
            };

            return _bl.CO_Readecuacion_ReporteOperacionNueva_Obtener(CodEmpresa, req);
        }
    }
}