using Galileo.DataBaseTier;
using Galileo.Models.GEN;

namespace Galileo.BusinessLogic
{
    public class FrmCcReportesAlCorteBl
    {
        readonly FrmCcReportesAlCorteDb DbfrmCC_ReportesAlCorte;

        public FrmCcReportesAlCorteBl(IConfiguration config)
        {
            DbfrmCC_ReportesAlCorte = new FrmCcReportesAlCorteDb(config);
        }

        public List<CCGenericList> CC_Periodos_Obtener(int CodEmpresa)
        {
            return DbfrmCC_ReportesAlCorte.CC_Periodos_Obtener(CodEmpresa);
        }

        public List<CCGenericList> CC_Instituciones_Obtener(int CodEmpresa)
        {
            return DbfrmCC_ReportesAlCorte.CC_Instituciones_Obtener(CodEmpresa);
        }
        public List<CCGenericList> CC_Profesiones_Obtener(int CodEmpresa)
        {
            return DbfrmCC_ReportesAlCorte.CC_Profesiones_Obtener(CodEmpresa);
        }
        public List<CCGenericList> CC_Sectores_Obtener(int CodEmpresa)
        {
            return DbfrmCC_ReportesAlCorte.CC_Sectores_Obtener(CodEmpresa);
        }
        public List<CCGenericList> CC_Zonas_Obtener(int CodEmpresa)
        {
            return DbfrmCC_ReportesAlCorte.CC_Zonas_Obtener(CodEmpresa);
        }
        public List<CCGenericList> CC_Estados_Persona_Obtener(int CodEmpresa)
        {
            return DbfrmCC_ReportesAlCorte.CC_Estados_Persona_Obtener(CodEmpresa);
        }
        public List<CCGenericList> CC_Garantias_Obtener(int CodEmpresa)
        {
            return DbfrmCC_ReportesAlCorte.CC_Garantias_Obtener(CodEmpresa);
        }

        public List<CCGenericList> CC_Carteras_Obtener(int CodEmpresa)
        {
            return DbfrmCC_ReportesAlCorte.CC_Carteras_Obtener(CodEmpresa);
        }

        public List<CCGenericList> CC_Oficinas_Obtener(int CodEmpresa)
        {
            return DbfrmCC_ReportesAlCorte.CC_Oficinas_Obtener(CodEmpresa);
        }

        public List<CCGenericList> CC_Estados_Civiles_Obtener(int CodEmpresa)
        {
            return DbfrmCC_ReportesAlCorte.CC_Estados_Civiles_Obtener(CodEmpresa);
        }

        public List<CCGenericList> CC_Estados_Laborales_Obtener(int CodEmpresa)
        {
            return DbfrmCC_ReportesAlCorte.CC_Estados_Laborales_Obtener(CodEmpresa);
        }

        public List<CCGenericList> CC_Provincias_Obtener(int CodEmpresa)
        {
            return DbfrmCC_ReportesAlCorte.CC_Provincias_Obtener(CodEmpresa);
        }

        public List<CCGenericList> CC_Cantones_Obtener(int CodEmpresa, int Provincia)
        {
            return DbfrmCC_ReportesAlCorte.CC_Cantones_Obtener(CodEmpresa, Provincia);
        }

        public List<CCGenericList> CC_Distritos_Obtener(int CodEmpresa, int Provincia, int Canton)
        {
            return DbfrmCC_ReportesAlCorte.CC_Distritos_Obtener(CodEmpresa, Provincia, Canton);
        }

        public List<CCGenericList> CC_Catalogo_Obtener(int CodEmpresa)
        {
            return DbfrmCC_ReportesAlCorte.CC_Catalogo_Obtener(CodEmpresa);
        }

        public List<CCGenericList> CC_Catalogo_Destinos_Obtener(int CodEmpresa, string? CodCatalgo)
        {
            return DbfrmCC_ReportesAlCorte.CC_Catalogo_Destinos_Obtener(CodEmpresa, CodCatalgo);
        }

        public List<CCGenericList> CC_Catalogo_Grupos_Obtener(int CodEmpresa, string? CodCatalgo)
        {
            return DbfrmCC_ReportesAlCorte.CC_Catalogo_Grupos_Obtener(CodEmpresa, CodCatalgo);
        }

        public List<CCGenericList> CC_Departamentos_Obtener(int CodEmpresa, string? CodInstitucion)
        {
            return DbfrmCC_ReportesAlCorte.CC_Departamentos_Obtener(CodEmpresa, CodInstitucion);
        }

        public List<CCGenericList> CC_Secciones_Obtener(int CodEmpresa, string? CodInstitucion, string? CodDepartamento)
        {
            return DbfrmCC_ReportesAlCorte.CC_Secciones_Obtener(CodEmpresa, CodInstitucion, CodDepartamento);
        }

        public List<CbrAnalisisCubosData> CbrAnalisis_Cubos_SP(int CodEmpresa, string nombreSP, int Anio, int Mes)
        {
            return DbfrmCC_ReportesAlCorte.CbrAnalisis_Cubos_SP(CodEmpresa, nombreSP, Anio, Mes);
        }

        public List<CbrEstimacionData> CbrEstimacion_SP(int CodEmpresa, string nombreSP, int Anio, int Mes)
        {
            return DbfrmCC_ReportesAlCorte.CbrEstimacion_SP(CodEmpresa, nombreSP, Anio, Mes);
        }

    }
}