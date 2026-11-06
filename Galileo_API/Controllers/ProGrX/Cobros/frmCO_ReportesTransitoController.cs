using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Cobros;
using Galileo_API.Models.ProGrX.Cobros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Cobros
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCoReportesTransitoController : ControllerBase
    {
        private readonly FrmCoReportesTransitoBL _bl;

        public FrmCoReportesTransitoController(IConfiguration config)
        {
            _bl = new FrmCoReportesTransitoBL(config);
        }

        [HttpGet("CO_ReportesTransito_Catalogo_Obtener")]
        public ErrorDto<List<CoReporteItemDto>> CO_ReportesTransito_Catalogo_Obtener(int CodEmpresa) => _bl.CO_ReportesTransito_Catalogo_Obtener(CodEmpresa);

        [HttpGet("CO_ReportesTransito_Lineas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CO_ReportesTransito_Lineas_Obtener(int CodEmpresa, string? texto) => _bl.CO_ReportesTransito_Lineas_Obtener(CodEmpresa, texto);

        [HttpGet("CO_ReportesTransito_LineaDescripcion_Obtener")]
        public ErrorDto<CoReporteCodigoDescripcionDto> CO_ReportesTransito_LineaDescripcion_Obtener(int CodEmpresa, string codigo) => _bl.CO_ReportesTransito_LineaDescripcion_Obtener(CodEmpresa, codigo);

        [HttpGet("CO_ReportesTransito_Recursos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CO_ReportesTransito_Recursos_Obtener(int CodEmpresa, string? codigo, bool todasLineas) => _bl.CO_ReportesTransito_Recursos_Obtener(CodEmpresa, codigo, todasLineas);

        [HttpGet("CO_ReportesTransito_Destinos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CO_ReportesTransito_Destinos_Obtener(int CodEmpresa, string? codigo, bool todasLineas) => _bl.CO_ReportesTransito_Destinos_Obtener(CodEmpresa, codigo, todasLineas);

        [HttpGet("CO_ReportesTransito_Instituciones_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CO_ReportesTransito_Instituciones_Obtener(int CodEmpresa) => _bl.CO_ReportesTransito_Instituciones_Obtener(CodEmpresa);

        [HttpGet("CO_ReportesTransito_Deductoras_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CO_ReportesTransito_Deductoras_Obtener(int CodEmpresa, int? codInstitucion) => _bl.CO_ReportesTransito_Deductoras_Obtener(CodEmpresa, codInstitucion);

        [HttpGet("CO_ReportesTransito_Garantias_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CO_ReportesTransito_Garantias_Obtener(int CodEmpresa) => _bl.CO_ReportesTransito_Garantias_Obtener(CodEmpresa);

        [HttpGet("CO_ReportesTransito_Oficinas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CO_ReportesTransito_Oficinas_Obtener(int CodEmpresa) => _bl.CO_ReportesTransito_Oficinas_Obtener(CodEmpresa);

        [HttpGet("CO_ReportesTransito_Estados_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CO_ReportesTransito_Estados_Obtener(int CodEmpresa) => _bl.CO_ReportesTransito_Estados_Obtener(CodEmpresa);

        [HttpGet("CO_ReportesTransito_Carteras_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CO_ReportesTransito_Carteras_Obtener(int CodEmpresa) => _bl.CO_ReportesTransito_Carteras_Obtener(CodEmpresa);
    }
}
