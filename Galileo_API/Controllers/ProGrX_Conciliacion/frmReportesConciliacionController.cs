using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Conciliacion;
using Galileo_API.Models.ProGrX.Conciliacion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Conciliacion
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCCReportesEstudioController : ControllerBase
    {
        private readonly FrmCcReportesEstudioBL _bl;

        public FrmCCReportesEstudioController(IConfiguration config)
        {
            _bl = new FrmCcReportesEstudioBL(config);
        }

        [HttpGet("CC_ReportesEstudio_Auxiliares_Inicial_Obtener")]
        [Authorize]
        public ErrorDto<CcReportesEstudioAuxiliaresInicialDto> CC_ReportesEstudio_Auxiliares_Inicial_Obtener(int CodEmpresa, int codContabilidad)
        {
            return _bl.CC_ReportesEstudio_Auxiliares_Inicial_Obtener(CodEmpresa, codContabilidad);
        }

        [HttpGet("CC_ReportesEstudio_Periodos_Dropdown_Obtener")]
        [Authorize]
        public ErrorDto<List<CcReportesEstudioPeriodoDto>> CC_ReportesEstudio_Periodos_Dropdown_Obtener(int CodEmpresa)
        {
            return _bl.CC_ReportesEstudio_Periodos_Dropdown_Obtener(CodEmpresa);
        }

        [HttpGet("CC_ReportesEstudio_Periodo_Obtener")]
        [Authorize]
        public ErrorDto<CcReportesEstudioPeriodoData> CC_ReportesEstudio_Periodo_Obtener(int CodEmpresa, int idPerHistorico)
        {
            return _bl.CC_ReportesEstudio_Periodo_Obtener(CodEmpresa, idPerHistorico);
        }

        [HttpGet("CC_ReportesEstudio_FechaServidor_Obtener")]
        [Authorize]
        public ErrorDto<DateTime> CC_ReportesEstudio_FechaServidor_Obtener(int CodEmpresa)
        {
            return _bl.CC_ReportesEstudio_FechaServidor_Obtener(CodEmpresa);
        }

        [HttpGet("CC_ReportesEstudio_Garantias_Dropdown_Obtener")]
        [Authorize]
        public ErrorDto<List<DropDownListaGenericaModel>> CC_ReportesEstudio_Garantias_Dropdown_Obtener(int CodEmpresa)
        {
            return _bl.CC_ReportesEstudio_Garantias_Dropdown_Obtener(CodEmpresa);
        }

        [HttpGet("CC_ReportesEstudio_Divisas_Dropdown_Obtener")]
        [Authorize]
        public ErrorDto<List<DropDownListaGenericaModel>> CC_ReportesEstudio_Divisas_Dropdown_Obtener(int CodEmpresa, int codContabilidad)
        {
            return _bl.CC_ReportesEstudio_Divisas_Dropdown_Obtener(CodEmpresa, codContabilidad);
        }

        [HttpGet("CC_ReportesEstudio_Operadoras_Dropdown_Obtener")]
        [Authorize]
        public ErrorDto<List<DropDownListaGenericaModel>> CC_ReportesEstudio_Operadoras_Dropdown_Obtener(int CodEmpresa)
        {
            return _bl.CC_ReportesEstudio_Operadoras_Dropdown_Obtener(CodEmpresa);
        }

        [HttpGet("CC_ReportesEstudio_GruposFondos_Dropdown_Obtener")]
        [Authorize]
        public ErrorDto<List<DropDownListaGenericaModel>> CC_ReportesEstudio_GruposFondos_Dropdown_Obtener(int CodEmpresa)
        {
            return _bl.CC_ReportesEstudio_GruposFondos_Dropdown_Obtener(CodEmpresa);
        }

        [HttpGet("CC_ReportesEstudio_Instituciones_Obtener")]
        [Authorize]
        public ErrorDto<List<DropDownListaGenericaModel>> CC_ReportesEstudio_Instituciones_Obtener(int CodEmpresa, string? texto)
        {
            return _bl.CC_ReportesEstudio_Instituciones_Obtener(CodEmpresa, texto);
        }

        [HttpGet("CC_ReportesEstudio_Lineas_Obtener")]
        [Authorize]
        public ErrorDto<List<DropDownListaGenericaModel>> CC_ReportesEstudio_Lineas_Obtener(int CodEmpresa, string? texto)
        {
            return _bl.CC_ReportesEstudio_Lineas_Obtener(CodEmpresa, texto);
        }

        [HttpGet("CC_ReportesEstudio_Destinos_Obtener")]
        [Authorize]
        public ErrorDto<List<DropDownListaGenericaModel>> CC_ReportesEstudio_Destinos_Obtener(int CodEmpresa, string? texto)
        {
            return _bl.CC_ReportesEstudio_Destinos_Obtener(CodEmpresa, texto);
        }

        [HttpGet("CC_ReportesEstudio_Recursos_Obtener")]
        [Authorize]
        public ErrorDto<List<DropDownListaGenericaModel>> CC_ReportesEstudio_Recursos_Obtener(int CodEmpresa, string? texto)
        {
            return _bl.CC_ReportesEstudio_Recursos_Obtener(CodEmpresa, texto);
        }

        [HttpGet("CC_ReportesEstudio_Planes_Obtener")]
        [Authorize]
        public ErrorDto<List<DropDownListaGenericaModel>> CC_ReportesEstudio_Planes_Obtener(int CodEmpresa, int codOperadora, string? texto)
        {
            return _bl.CC_ReportesEstudio_Planes_Obtener(CodEmpresa, codOperadora, texto);
        }

        [HttpGet("CC_ReportesEstudio_Cuenta_Descripcion_Obtener")]
        [Authorize]
        public ErrorDto<CcReportesEstudioCuentaDto> CC_ReportesEstudio_Cuenta_Descripcion_Obtener(int CodEmpresa, string? cuenta, int codContabilidad)
        {
            return _bl.CC_ReportesEstudio_Cuenta_Descripcion_Obtener(CodEmpresa, cuenta, codContabilidad);
        }

        [HttpPost("CC_ReportesEstudio_Auxiliar_Generar")]
        [Authorize]
        public ErrorDto<CcReportesEstudioAuxiliarGenerarResult> CC_ReportesEstudio_Auxiliar_Generar(int CodEmpresa, [FromBody] CcReportesEstudioAuxiliarGenerarRequest? request)
        {
            return _bl.CC_ReportesEstudio_Auxiliar_Generar(CodEmpresa, request);
        }
        [HttpGet("CC_ReportesEstudio_Carteras_Lista_Obtener")]
        [Authorize]
        public ErrorDto<List<DropDownListaGenericaModel>> CC_ReportesEstudio_Carteras_Lista_Obtener(int CodEmpresa)
        {
            return _bl.CC_ReportesEstudio_Carteras_Lista_Obtener(CodEmpresa);
        }
        [HttpPost("CC_ReportesEstudio_Especial_Generar")]
        [Authorize]
        public ErrorDto<CcReportesEstudioEspecialReporteDto> CC_ReportesEstudio_Especial_Generar(int CodEmpresa, [FromBody] CcReportesEstudioEspecialRequest? request)
        {
            return _bl.CC_ReportesEstudio_Especial_Generar(CodEmpresa, request);
        }
    }
}