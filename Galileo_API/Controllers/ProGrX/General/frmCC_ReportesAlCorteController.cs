using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.GEN;

namespace Galileo.Controllers
{
    [Route("api/frmCC_ReportesAlCorte")]
    [ApiController]
    public class FrmCcReportesAlCorteController : ControllerBase
    {
        readonly FrmCcReportesAlCorteBl BL_CC_ReportesAlCorte;
        public FrmCcReportesAlCorteController(IConfiguration config)
        {
            BL_CC_ReportesAlCorte = new FrmCcReportesAlCorteBl(config);
        }

        [HttpGet("CC_Periodos_Obtener")]
        public List<CCGenericList> CC_Periodos_Obtener(int CodEmpresa)
        {
            return BL_CC_ReportesAlCorte.CC_Periodos_Obtener(CodEmpresa);
        }

        [HttpGet("CC_Instituciones_Obtener")]
        public List<CCGenericList> CC_Instituciones_Obtener(int CodEmpresa)
        {
            return BL_CC_ReportesAlCorte.CC_Instituciones_Obtener(CodEmpresa);
        }

        [HttpGet("CC_Profesiones_Obtener")]
        public List<CCGenericList> CC_Profesiones_Obtener(int CodEmpresa)
        {
            return BL_CC_ReportesAlCorte.CC_Profesiones_Obtener(CodEmpresa);
        }

        [HttpGet("CC_Sectores_Obtener")]
        public List<CCGenericList> CC_Sectores_Obtener(int CodEmpresa)
        {
            return BL_CC_ReportesAlCorte.CC_Sectores_Obtener(CodEmpresa);
        }

        [HttpGet("CC_Zonas_Obtener")]
        public List<CCGenericList> CC_Zonas_Obtener(int CodEmpresa)
        {
            return BL_CC_ReportesAlCorte.CC_Zonas_Obtener(CodEmpresa);
        }

        [HttpGet("CC_Estados_Persona_Obtener")]
        public List<CCGenericList> CC_Estados_Persona_Obtener(int CodEmpresa)
        {
            return BL_CC_ReportesAlCorte.CC_Estados_Persona_Obtener(CodEmpresa);
        }

        [HttpGet("CC_Garantias_Obtener")]
        public List<CCGenericList> CC_Garantias_Obtener(int CodEmpresa)
        {
            return BL_CC_ReportesAlCorte.CC_Garantias_Obtener(CodEmpresa);
        }

        [HttpGet("CC_Carteras_Obtener")]
        public List<CCGenericList> CC_Carteras_Obtener(int CodEmpresa)
        {
            return BL_CC_ReportesAlCorte.CC_Carteras_Obtener(CodEmpresa);
        }

        [HttpGet("CC_Oficinas_Obtener")]
        public List<CCGenericList> CC_Oficinas_Obtener(int CodEmpresa)
        {
            return BL_CC_ReportesAlCorte.CC_Oficinas_Obtener(CodEmpresa);
        }

        [HttpGet("CC_Estados_Civiles_Obtener")]
        public List<CCGenericList> CC_Estados_Civiles_Obtener(int CodEmpresa)
        {
            return BL_CC_ReportesAlCorte.CC_Estados_Civiles_Obtener(CodEmpresa);
        }

        [HttpGet("CC_Estados_Laborales_Obtener")]
        public List<CCGenericList> CC_Estados_Laborales_Obtener(int CodEmpresa)
        {
            return BL_CC_ReportesAlCorte.CC_Estados_Laborales_Obtener(CodEmpresa);
        }

        [HttpGet("CC_Provincias_Obtener")]
        public List<CCGenericList> CC_Provincias_Obtener(int CodEmpresa)
        {
            return BL_CC_ReportesAlCorte.CC_Provincias_Obtener(CodEmpresa);
        }

        [HttpGet("CC_Cantones_Obtener")]
        public List<CCGenericList> CC_Cantones_Obtener(int CodEmpresa, int Provincia)
        {
            return BL_CC_ReportesAlCorte.CC_Cantones_Obtener(CodEmpresa, Provincia);
        }

        [HttpGet("CC_Distritos_Obtener")]
        public List<CCGenericList> CC_Distritos_Obtener(int CodEmpresa, int Provincia, int Canton)
        {
            return BL_CC_ReportesAlCorte.CC_Distritos_Obtener(CodEmpresa, Provincia, Canton);
        }

        [HttpGet("CC_Catalogo_Obtener")]
        public List<CCGenericList> CC_Catalogo_Obtener(int CodEmpresa)
        {
            return BL_CC_ReportesAlCorte.CC_Catalogo_Obtener(CodEmpresa);
        }

        [HttpGet("CC_Catalogo_Destinos_Obtener")]
        public List<CCGenericList> CC_Catalogo_Destinos_Obtener(int CodEmpresa, string? CodCatalgo)
        {
            return BL_CC_ReportesAlCorte.CC_Catalogo_Destinos_Obtener(CodEmpresa, CodCatalgo);
        }

        [HttpGet("CC_Catalogo_Grupos_Obtener")]
        public List<CCGenericList> CC_Catalogo_Grupos_Obtener(int CodEmpresa, string? CodCatalgo)
        {
            return BL_CC_ReportesAlCorte.CC_Catalogo_Grupos_Obtener(CodEmpresa, CodCatalgo);
        }

        [HttpGet("CC_Departamentos_Obtener")]
        public List<CCGenericList> CC_Departamentos_Obtener(int CodEmpresa, string? CodInstitucion)
        {
            return BL_CC_ReportesAlCorte.CC_Departamentos_Obtener(CodEmpresa, CodInstitucion);
        }

        [HttpGet("CC_Secciones_Obtener")]
        public List<CCGenericList> CC_Secciones_Obtener(int CodEmpresa, string? CodInstitucion, string? CodDepartamento)
        {
            return BL_CC_ReportesAlCorte.CC_Secciones_Obtener(CodEmpresa, CodInstitucion, CodDepartamento);
        }

        [HttpGet("CbrAnalisis_Cubos_SP")]
        public List<CbrAnalisisCubosData> CbrAnalisis_Cubos_SP(int CodEmpresa, string nombreSP, int Anio, int Mes)
        {
            return BL_CC_ReportesAlCorte.CbrAnalisis_Cubos_SP(CodEmpresa, nombreSP, Anio, Mes);
        }

        [HttpGet("CbrEstimacion_SP")]
        public List<CbrEstimacionData> CbrEstimacion_SP(int CodEmpresa, string nombreSP, int Anio, int Mes)
        {
            return BL_CC_ReportesAlCorte.CbrEstimacion_SP(CodEmpresa, nombreSP, Anio, Mes);
        }
    }
}