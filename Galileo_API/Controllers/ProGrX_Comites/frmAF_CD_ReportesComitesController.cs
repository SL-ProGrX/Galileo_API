using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Comites;
using Galileo_API.Models.ProGrX_Comites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Comites
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAfCdReportesComitesController : ControllerBase
    {
        private readonly FrmAfCdReportesComitesBl _bl;

        public FrmAfCdReportesComitesController(IConfiguration config)
        {
            _bl = new FrmAfCdReportesComitesBl(config);
        }

        [Authorize]
        [HttpGet("AF_CD_ReportesComites_Catalogo_Obtener")]
        public ErrorDto<List<AfCdReporteCatalogoDto>> AF_CD_ReportesComites_Catalogo_Obtener(int CodEmpresa)
        {
            return _bl.AF_CD_ReportesComites_Catalogo_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("AF_CD_ReportesComites_Definicion_Obtener")]
        public ErrorDto<AfCdReporteDefinicionDto> AF_CD_ReportesComites_Definicion_Obtener(int CodEmpresa, string codigo)
        {
            return _bl.AF_CD_ReportesComites_Definicion_Obtener(CodEmpresa, codigo);
        }

        [Authorize]
        [HttpGet("AF_CD_ReportesComites_TiposReporte_Obtener")]
        public ErrorDto<List<AfCdReporteTipoDto>> AF_CD_ReportesComites_TiposReporte_Obtener(int CodEmpresa, string codigo)
        {
            return _bl.AF_CD_ReportesComites_TiposReporte_Obtener(CodEmpresa, codigo);
        }

        [Authorize]
        [HttpGet("AF_CD_ReportesComites_ParametrosIniciales_Obtener")]
        public ErrorDto<AfCdReportesComitesParametrosInicialesDto> AF_CD_ReportesComites_ParametrosIniciales_Obtener(int CodEmpresa)
        {
            return _bl.AF_CD_ReportesComites_ParametrosIniciales_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("AF_CD_Comites_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_CD_Comites_Dropdown_Obtener(int CodEmpresa, string? filtro)
        {
            return _bl.AF_CD_Comites_Dropdown_Obtener(CodEmpresa, filtro);
        }

        [Authorize]
        [HttpGet("AF_CD_Actividades_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_CD_Actividades_Dropdown_Obtener(int CodEmpresa, string? filtro)
        {
            return _bl.AF_CD_Actividades_Dropdown_Obtener(CodEmpresa, filtro);
        }

        [Authorize]
        [HttpGet("AF_CD_Promotores_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_CD_Promotores_Dropdown_Obtener(int CodEmpresa, string? filtro)
        {
            return _bl.AF_CD_Promotores_Dropdown_Obtener(CodEmpresa, filtro);
        }
    }
}