using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Pasivos;
using Galileo_API.Models.ProGrX_Pasivos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Pasivos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCrApaLineasController : ControllerBase
    {
        private readonly FrmCrApaLineasBL _BL;

        public FrmCrApaLineasController(IConfiguration config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            _BL = new FrmCrApaLineasBL(config);
        }

        [Authorize]
        [HttpGet("CR_APA_Lineas_Catalogos_Obtener")]
        public ErrorDto<FrmCrApaLineaCatalogosDto> CR_APA_Lineas_Catalogos_Obtener(int codEmpresa)
        {
            return _BL.CR_APA_Lineas_Catalogos_Obtener(codEmpresa);
        }

        [Authorize]
        [HttpGet("CR_APA_Lineas_CentrosCosto_Obtener")]
        public ErrorDto<List<FrmCrApaLineaCatalogoDto>> CR_APA_Lineas_CentrosCosto_Obtener(int codEmpresa, string cod_unidad)
        {
            return _BL.CR_APA_Lineas_CentrosCosto_Obtener(codEmpresa, cod_unidad);
        }

        [Authorize]
        [HttpGet("CR_APA_Lineas_Consultar")]
        public ErrorDto<List<FrmCrApaLineaGridDto>> CR_APA_Lineas_Consultar(
            int codEmpresa, [FromQuery] FrmCrApaLineaConsultaRequest request)
        {
            return _BL.CR_APA_Lineas_Consultar(codEmpresa, request);
        }

        [Authorize]
        [HttpGet("CR_APA_Lineas_Obtener")]
        public ErrorDto<FrmCrApaLineaDatosDto> CR_APA_Lineas_Obtener(int codEmpresa, int cod_linea)
        {
            return _BL.CR_APA_Lineas_Obtener(codEmpresa, cod_linea);
        }

        [Authorize]
        [HttpPost("CR_APA_Lineas_Guardar")]
        public ErrorDto<FrmCrApaLineaGuardarResultadoDto> CR_APA_Lineas_Guardar(
            int codEmpresa, [FromBody] FrmCrApaLineaGuardarRequest request)
        {
            return _BL.CR_APA_Lineas_Guardar(codEmpresa, request);
        }
    }
}
