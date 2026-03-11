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
    public class FrmCoReportesController : ControllerBase
    {
        private readonly FrmCoReportesBL BL;

        public FrmCoReportesController(IConfiguration config)
        {
            BL = new FrmCoReportesBL(config);
        }

        [Authorize]
        [HttpGet("CO_Reportes_Catalogo_Obtener")]
        public ErrorDto<List<CoReporteItemDto>> CO_Reportes_Catalogo_Obtener(int CodEmpresa)
        {
            return BL.CO_Reportes_Catalogo_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("CO_Lineas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CO_Lineas_Obtener(int CodEmpresa, string? texto)
        {
            return BL.CO_Lineas_Obtener(CodEmpresa, texto);
        }

        [Authorize]
        [HttpGet("CO_Linea_Descripcion_Obtener")]
        public ErrorDto<CoReporteCodigoDescripcionDto> CO_Linea_Descripcion_Obtener(int CodEmpresa, string codigo)
        {
            return BL.CO_Linea_Descripcion_Obtener(CodEmpresa, codigo);
        }

        [Authorize]
        [HttpGet("CO_Recursos_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CO_Recursos_Dropdown_Obtener(int CodEmpresa, string? codigo, bool todasLineas)
        {
            return BL.CO_Recursos_Dropdown_Obtener(CodEmpresa, codigo, todasLineas);
        }

        [Authorize]
        [HttpGet("CO_Destinos_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CO_Destinos_Dropdown_Obtener(int CodEmpresa, string? codigo, bool todasLineas)
        {
            return BL.CO_Destinos_Dropdown_Obtener(CodEmpresa, codigo, todasLineas);
        }

        [Authorize]
        [HttpGet("CO_Comites_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CO_Comites_Dropdown_Obtener(int CodEmpresa)
        {
            return BL.CO_Comites_Dropdown_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("CO_Instituciones_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CO_Instituciones_Dropdown_Obtener(int CodEmpresa)
        {
            return BL.CO_Instituciones_Dropdown_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("CO_Deductoras_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CO_Deductoras_Dropdown_Obtener(int CodEmpresa, int? codInstitucion)
        {
            return BL.CO_Deductoras_Dropdown_Obtener(CodEmpresa, codInstitucion);
        }

        [Authorize]
        [HttpGet("CO_Divisas_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CO_Divisas_Dropdown_Obtener(int CodEmpresa, int gEnlace)
        {
            return BL.CO_Divisas_Dropdown_Obtener(CodEmpresa, gEnlace);
        }

        [Authorize]
        [HttpGet("CO_EstadosLaborales_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CO_EstadosLaborales_Dropdown_Obtener(int CodEmpresa)
        {
            return BL.CO_EstadosLaborales_Dropdown_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("CO_Gestiona_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CO_Gestiona_Dropdown_Obtener(int CodEmpresa)
        {
            return BL.CO_Gestiona_Dropdown_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("CO_Garantias_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CO_Garantias_Dropdown_Obtener(int CodEmpresa)
        {
            return BL.CO_Garantias_Dropdown_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("CO_Antiguedades_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CO_Antiguedades_Dropdown_Obtener(int CodEmpresa)
        {
            return BL.CO_Antiguedades_Dropdown_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("CO_Carteras_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CO_Carteras_Dropdown_Obtener(int CodEmpresa)
        {
            return BL.CO_Carteras_Dropdown_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("CO_EstadosPersona_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CO_EstadosPersona_Dropdown_Obtener(int CodEmpresa)
        {
            return BL.CO_EstadosPersona_Dropdown_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpPost("CO_Reportes_Cubo_Procesar")]
        public ErrorDto CO_Reportes_Cubo_Procesar(int CodEmpresa, string usuario)
        {
            return BL.CO_Reportes_Cubo_Procesar(CodEmpresa, usuario);
        }
    }
}