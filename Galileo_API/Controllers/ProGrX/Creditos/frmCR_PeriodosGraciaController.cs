using Galileo.Models;
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
    public class FrmCrPeriodosGraciaController : ControllerBase
    {
        private readonly FrmCrPeriodosGraciaBl _bl;

        public FrmCrPeriodosGraciaController(IConfiguration config)
        {
            _bl = new FrmCrPeriodosGraciaBl(config);
        }

        [HttpGet("CrPeriodosGracia_Garantias_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CrPeriodosGracia_Garantias_Obtener(int codEmpresa)
            => _bl.CrPeriodosGracia_Garantias_Obtener(codEmpresa);

        [HttpGet("CrPeriodosGracia_Divisas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CrPeriodosGracia_Divisas_Obtener(int codEmpresa)
            => _bl.CrPeriodosGracia_Divisas_Obtener(codEmpresa);

        [HttpGet("CrPeriodosGracia_Recursos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CrPeriodosGracia_Recursos_Obtener(
            int codEmpresa,
            bool lineas,
            string? codigo)
            => _bl.CrPeriodosGracia_Recursos_Obtener(codEmpresa, lineas, codigo);

        [HttpGet("CrPeriodosGracia_Destinos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CrPeriodosGracia_Destinos_Obtener(
            int codEmpresa,
            bool lineas,
            string? codigo)
            => _bl.CrPeriodosGracia_Destinos_Obtener(codEmpresa, lineas, codigo);

        [HttpGet("CrPeriodosGracia_Instituciones_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CrPeriodosGracia_Instituciones_Obtener(int codEmpresa)
            => _bl.CrPeriodosGracia_Instituciones_Obtener(codEmpresa);

        [HttpGet("CrPeriodosGracia_Deductoras_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CrPeriodosGracia_Deductoras_Obtener(
            int codEmpresa,
            bool todos,
            string? codInstitucion)
            => _bl.CrPeriodosGracia_Deductoras_Obtener(codEmpresa, todos, codInstitucion);

        [HttpGet("CrPeriodosGracia_EstadosPersona_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CrPeriodosGracia_EstadosPersona_Obtener(int codEmpresa)
            => _bl.CrPeriodosGracia_EstadosPersona_Obtener(codEmpresa);

        [HttpGet("CrPeriodosGracia_EstadosLaborales_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CrPeriodosGracia_EstadosLaborales_Obtener(int codEmpresa)
            => _bl.CrPeriodosGracia_EstadosLaborales_Obtener(codEmpresa);

        [HttpGet("CrPeriodosGracia_Lineas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CrPeriodosGracia_Lineas_Obtener(int codEmpresa)
            => _bl.CrPeriodosGracia_Lineas_Obtener(codEmpresa);

        [HttpPost("CrPeriodosGracia_Consulta_Obtener")]
        public ErrorDto<List<dynamic>> CrPeriodosGracia_Consulta_Obtener(
            int codEmpresa,
            [FromBody] CrPeriodosGraciaConsultaRequest request)
            => _bl.CrPeriodosGracia_Consulta_Obtener(codEmpresa, request);

        [HttpPost("CrPeriodosGracia_Aplicar_Ejecutar")]
        public ErrorDto CrPeriodosGracia_Aplicar_Ejecutar(
            int codEmpresa,
            [FromBody] CrPeriodosGraciaConsultaRequest request)
            => _bl.CrPeriodosGracia_Aplicar_Ejecutar(codEmpresa, request);
    }
}
