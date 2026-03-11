using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cobros;
using Galileo_API.BusinessTier.ProGrX.Cobros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Cobros
{
    [ApiController]
    [Route("api/[controller]")]
    public class FrmCOUnificacionCuotasController : ControllerBase
    {
        private readonly FrmCOUnificacionCuotasBL _bl;

        public FrmCOUnificacionCuotasController(IConfiguration config)
        {
            _bl = new FrmCOUnificacionCuotasBL(config);
        }
        [Authorize]
        [HttpGet("CO_UnificacionCuotas_Codigos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CO_UnificacionCuotas_Codigos_Obtener([FromQuery] int CodEmpresa,[FromQuery] string? texto)
        {
            return _bl.CO_UnificacionCuotas_Codigos_Obtener(CodEmpresa, texto);
        }
        [Authorize]
        [HttpGet("Co_UnificacionCuotas_Lista_Obtener")]
        public ErrorDto<CoUnificacionCuotasListaResult> Co_UnificacionCuotas_Lista_Obtener([FromQuery] int CodEmpresa, [FromQuery] string jfiltros)
        {
            return _bl.Co_UnificacionCuotas_Lista_Obtener(CodEmpresa, jfiltros);
        }

        [Authorize]
        [HttpGet("Co_UnificacionCuotas_Lista_Export")]
        public ErrorDto<CoUnificacionCuotasListaResult> Co_UnificacionCuotas_Lista_Export([FromQuery] int CodEmpresa, [FromQuery] string jfiltros)
        {
            return _bl.Co_UnificacionCuotas_Lista_Export(CodEmpresa, jfiltros);
        }

        [Authorize]
        [HttpPost("Co_UnificacionCuotas_Unificar")]
        public ErrorDto<CoUnificacionCuotasUnificarResponse> Co_UnificacionCuotas_Unificar([FromQuery] int CodEmpresa, [FromBody] CoUnificacionCuotasUnificarRequest req)
        {
            return _bl.Co_UnificacionCuotas_Unificar(CodEmpresa, req);
        }
    }
}