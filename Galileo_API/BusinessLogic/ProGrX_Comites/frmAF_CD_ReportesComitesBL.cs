using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Comites;
using Galileo_API.Models.ProGrX_Comites;

namespace Galileo_API.BusinessLogic.ProGrX_Comites
{
    public class FrmAF_CD_ReportesComitesBL
    {
        private readonly FrmAF_CD_ReportesComitesDB _db;

        public FrmAF_CD_ReportesComitesBL(IConfiguration config)
        {
            _db = new FrmAF_CD_ReportesComitesDB(config);
        }

        public ErrorDto<List<AfCdReporteCatalogoDto>> AF_CD_ReportesComites_Catalogo_Obtener(int CodEmpresa)
        {
            return _db.AF_CD_ReportesComites_Catalogo_Obtener(CodEmpresa);
        }

        public ErrorDto<AfCdReporteDefinicionDto> AF_CD_ReportesComites_Definicion_Obtener(int CodEmpresa, string codigo)
        {
            return _db.AF_CD_ReportesComites_Definicion_Obtener(CodEmpresa, codigo);
        }

        public ErrorDto<List<AfCdReporteTipoDto>> AF_CD_ReportesComites_TiposReporte_Obtener(int CodEmpresa, string codigo)
        {
            return _db.AF_CD_ReportesComites_TiposReporte_Obtener(CodEmpresa, codigo);
        }

        public ErrorDto<AfCdReportesComitesParametrosInicialesDto> AF_CD_ReportesComites_ParametrosIniciales_Obtener(int CodEmpresa)
        {
            return _db.AF_CD_ReportesComites_ParametrosIniciales_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_CD_Comites_Dropdown_Obtener(int CodEmpresa, string? filtro)
        {
            return _db.AF_CD_Comites_Dropdown_Obtener(CodEmpresa, filtro);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_CD_Actividades_Dropdown_Obtener(int CodEmpresa, string? filtro)
        {
            return _db.AF_CD_Actividades_Dropdown_Obtener(CodEmpresa, filtro);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_CD_Promotores_Dropdown_Obtener(int CodEmpresa, string? filtro)
        {
            return _db.AF_CD_Promotores_Dropdown_Obtener(CodEmpresa, filtro);
        }
    }
}