using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Pasivos;
using Galileo_API.Models.ProGrX_Pasivos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Pasivos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCrApaLineasController : ControllerBase
    {
        private readonly FrmCrApaLineasBL _bl;

        public FrmCrApaLineasController(IConfiguration config) => _bl = new FrmCrApaLineasBL(config);

        [HttpGet("CR_APA_Lineas_Catalogos_Obtener")]
        public ErrorDto<FrmCrApaLineaCatalogosDto> CR_APA_Lineas_Catalogos_Obtener(int codEmpresa) =>
            _bl.CR_APA_Lineas_Catalogos_Obtener(codEmpresa);

        [HttpGet("CR_APA_Lineas_CentrosCosto_Obtener")]
        public ErrorDto<List<FrmCrApaLineaCatalogoDto>> CR_APA_Lineas_CentrosCosto_Obtener(int codEmpresa, string cod_unidad) =>
            _bl.CR_APA_Lineas_CentrosCosto_Obtener(codEmpresa, cod_unidad);

        [HttpGet("CR_APA_Lineas_Consultar")]
        public ErrorDto<List<FrmCrApaLineaGridDto>> CR_APA_Lineas_Consultar(
            int codEmpresa, [FromQuery] FrmCrApaLineaConsultaRequest request) =>
            _bl.CR_APA_Lineas_Consultar(codEmpresa, request);

        [HttpGet("CR_APA_Lineas_Obtener")]
        public ErrorDto<FrmCrApaLineaDatosDto> CR_APA_Lineas_Obtener(int codEmpresa, int cod_linea) =>
            _bl.CR_APA_Lineas_Obtener(codEmpresa, cod_linea);

        [HttpPost("CR_APA_Lineas_Guardar")]
        public ErrorDto<FrmCrApaLineaGuardarResultadoDto> CR_APA_Lineas_Guardar(
            int codEmpresa, FrmCrApaLineaGuardarRequest request) =>
            _bl.CR_APA_Lineas_Guardar(codEmpresa, request);
    }
}
