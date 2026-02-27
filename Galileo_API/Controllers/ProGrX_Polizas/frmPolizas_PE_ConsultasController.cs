using Galileo.Models.ERROR;
using Galileo.Models;
using Galileo_API.BusinessLogic.ProGrX_Polizas;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using static Galileo_API.Models.ProGrX_Polizas.FrmPolizasPeConsultasModels;

namespace Galileo_API.Controllers.ProGrX_Polizas
{


    [Route("api/[controller]")]
    [ApiController]

    public class FrmPolizasPeConsultasController : ControllerBase
    {

        private readonly FrmPolizasPeConsultasBL _bl;

        public FrmPolizasPeConsultasController(IConfiguration config)
        {
            _bl = new FrmPolizasPeConsultasBL(config);
        }


        [Authorize]
        [HttpGet("PolizasPeConsultas_EstadosPersona_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> PolizasPeConsultas_EstadosPersona_Obtener(int codEmpresa)
            => _bl.PolizasPeConsultas_EstadosPersona_Obtener(codEmpresa);

        [Authorize]
        [HttpGet("PolizasPeConsultas_Presentaciones_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> PolizasPeConsultas_Presentaciones_Obtener(int codEmpresa)
            => _bl.PolizasPeConsultas_Presentaciones_Obtener(codEmpresa);

        [Authorize]
        [HttpGet("PolizasPeConsultas_Modelos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> PolizasPeConsultas_Modelos_Obtener(int codEmpresa)
            => _bl.PolizasPeConsultas_Modelos_Obtener(codEmpresa);

        [Authorize]
        [HttpGet("PolizasPeConsultas_Combustibles_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> PolizasPeConsultas_Combustibles_Obtener(int codEmpresa)
            => _bl.PolizasPeConsultas_Combustibles_Obtener(codEmpresa);

        [Authorize]
        [HttpGet("PolizasPeConsultas_UnidadesPeso_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> PolizasPeConsultas_UnidadesPeso_Obtener(int codEmpresa)
            => _bl.PolizasPeConsultas_UnidadesPeso_Obtener(codEmpresa);

        [Authorize]
        [HttpGet("PolizasPeConsultas_UnidadesCapacidad_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> PolizasPeConsultas_UnidadesCapacidad_Obtener(int codEmpresa)
            => _bl.PolizasPeConsultas_UnidadesCapacidad_Obtener(codEmpresa);

        [Authorize]
        [HttpGet("PolizasPeConsultas_UnidadesCilindraje_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> PolizasPeConsultas_UnidadesCilindraje_Obtener(int codEmpresa)
            => _bl.PolizasPeConsultas_UnidadesCilindraje_Obtener(codEmpresa);

        [Authorize]
        [HttpPost("PolizasPeConsultas_Buscar")]
        public ErrorDto<PolizasPeConsultasBuscarResponseDto> PolizasPeConsultas_Buscar(int codEmpresa, bool esExportar, [FromBody] PolizasPeConsultasBuscarRequestDto request)
         => _bl.PolizasPeConsultas_Buscar(codEmpresa, esExportar, request);

    }

}
