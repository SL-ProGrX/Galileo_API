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
    public class FrmCrReportesMovController : ControllerBase
    {
        private readonly FrmCrReportesMovBl _bl;

        public FrmCrReportesMovController(IConfiguration config)
        {
            _bl = new FrmCrReportesMovBl(config);
        }

        [Authorize]
        [HttpGet("CrReportesMov_Documentos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CrReportesMov_Documentos_Obtener(int codEmpresa)
            => _bl.CrReportesMov_Documentos_Obtener(codEmpresa);

        [Authorize]
        [HttpGet("CrReportesMov_Conceptos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CrReportesMov_Conceptos_Obtener(int codEmpresa)
            => _bl.CrReportesMov_Conceptos_Obtener(codEmpresa);

        [Authorize]
        [HttpGet("CrReportesMov_Instituciones_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CrReportesMov_Instituciones_Obtener(int codEmpresa)
            => _bl.CrReportesMov_Instituciones_Obtener(codEmpresa);

        [Authorize]
        [HttpGet("CrReportesMov_Grupos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CrReportesMov_Grupos_Obtener(
            int codEmpresa,
            bool lineaActiva,
            string? codigo)
            => _bl.CrReportesMov_Grupos_Obtener(codEmpresa, lineaActiva, codigo);

        [Authorize]
        [HttpGet("CrReportesMov_Destinos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CrReportesMov_Destinos_Obtener(
            int codEmpresa,
            bool lineaActiva,
            string? codigo)
            => _bl.CrReportesMov_Destinos_Obtener(codEmpresa, lineaActiva, codigo);

        [Authorize]
        [HttpGet("CrReportesMov_Lineas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CrReportesMov_Lineas_Obtener(int codEmpresa)
            => _bl.CrReportesMov_Lineas_Obtener(codEmpresa);

        [Authorize]
        [HttpGet("CrReportesMov_Oficinas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CrReportesMov_Oficinas_Obtener(int codEmpresa)
            => _bl.CrReportesMov_Oficinas_Obtener(codEmpresa);

        [Authorize]
        [HttpGet("CrReportesMov_Garantias_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CrReportesMov_Garantias_Obtener(int codEmpresa)
            => _bl.CrReportesMov_Garantias_Obtener(codEmpresa);

        [Authorize]
        [HttpGet("CrReportesMov_Divisas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CrReportesMov_Divisas_Obtener(int codEmpresa)
            => _bl.CrReportesMov_Divisas_Obtener(codEmpresa);

        [Authorize]
        [HttpGet("CrReportesMov_Cargos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CrReportesMov_Cargos_Obtener(int codEmpresa)
            => _bl.CrReportesMov_Cargos_Obtener(codEmpresa);

        [Authorize]
        [HttpGet("CrReportesMov_Aseguradoras_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CrReportesMov_Aseguradoras_Obtener(int codEmpresa)
            => _bl.CrReportesMov_Aseguradoras_Obtener(codEmpresa);

        [Authorize]
        [HttpGet("CrReportesMov_Polizas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CrReportesMov_Polizas_Obtener(int codEmpresa)
            => _bl.CrReportesMov_Polizas_Obtener(codEmpresa);

        [Authorize]
        [HttpGet("CrReportesMov_Gestores_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CrReportesMov_Gestores_Obtener(int codEmpresa)
            => _bl.CrReportesMov_Gestores_Obtener(codEmpresa);

        [Authorize]
        [HttpPost("CrReportesMov_AnalisisCubo_Ejecutar")]
        public ErrorDto CrReportesMov_AnalisisCubo_Ejecutar(
            int codEmpresa,
            [FromBody] CrReportesMovAnalisisCuboRequest request)
            => _bl.CrReportesMov_AnalisisCubo_Ejecutar(codEmpresa, request);
    }
}
