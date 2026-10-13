using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.GEN;

namespace Galileo.BusinessLogic
{
    public class FrmCcReportesAlCorteBl
    {
        private readonly FrmCcReportesAlCorteDb _db;

        public FrmCcReportesAlCorteBl(
            IConfiguration config)
        {
            _db =
                new FrmCcReportesAlCorteDb(
                    config);
        }

        public ErrorDto<CcReportesAlCorteInicializacionData>
            CC_ReportesAlCorte_Inicializar(
                int codEmpresa)
        {
            return _db
                .CC_ReportesAlCorte_Inicializar(
                    codEmpresa);
        }

        public ErrorDto<List<CbrAnalisisCubosData>>
            CC_ReportesAlCorte_AnalisisCubos_Obtener(
                int codEmpresa,
                CcReportesAlCorteProcesoRequest request)
        {
            return _db
                .CC_ReportesAlCorte_AnalisisCubos_Obtener(
                    codEmpresa,
                    request);
        }

        public ErrorDto<List<CbrEstimacionData>>
            CC_ReportesAlCorte_Estimacion_Obtener(
                int codEmpresa,
                CcReportesAlCorteProcesoRequest request)
        {
            return _db
                .CC_ReportesAlCorte_Estimacion_Obtener(
                    codEmpresa,
                    request);
        }

        public List<CCGenericList>
            CC_Periodos_Obtener(
                int CodEmpresa)
        {
            return _db
                .CC_Periodos_Obtener(
                    CodEmpresa);
        }

        public List<CCGenericList>
            CC_Instituciones_Obtener(
                int CodEmpresa)
        {
            return _db
                .CC_Instituciones_Obtener(
                    CodEmpresa);
        }

        public List<CCGenericList>
            CC_Profesiones_Obtener(
                int CodEmpresa)
        {
            return _db
                .CC_Profesiones_Obtener(
                    CodEmpresa);
        }

        public List<CCGenericList>
            CC_Sectores_Obtener(
                int CodEmpresa)
        {
            return _db
                .CC_Sectores_Obtener(
                    CodEmpresa);
        }

        public List<CCGenericList>
            CC_Zonas_Obtener(
                int CodEmpresa)
        {
            return _db
                .CC_Zonas_Obtener(
                    CodEmpresa);
        }

        public List<CCGenericList>
            CC_Estados_Persona_Obtener(
                int CodEmpresa)
        {
            return _db
                .CC_Estados_Persona_Obtener(
                    CodEmpresa);
        }

        public List<CCGenericList>
            CC_Garantias_Obtener(
                int CodEmpresa)
        {
            return _db
                .CC_Garantias_Obtener(
                    CodEmpresa);
        }

        public List<CCGenericList>
            CC_Carteras_Obtener(
                int CodEmpresa)
        {
            return _db
                .CC_Carteras_Obtener(
                    CodEmpresa);
        }

        public List<CCGenericList>
            CC_Oficinas_Obtener(
                int CodEmpresa)
        {
            return _db
                .CC_Oficinas_Obtener(
                    CodEmpresa);
        }

        public List<CCGenericList>
            CC_Estados_Civiles_Obtener(
                int CodEmpresa)
        {
            return _db
                .CC_Estados_Civiles_Obtener(
                    CodEmpresa);
        }

        public List<CCGenericList>
            CC_Estados_Laborales_Obtener(
                int CodEmpresa)
        {
            return _db
                .CC_Estados_Laborales_Obtener(
                    CodEmpresa);
        }

        public List<CCGenericList>
            CC_Provincias_Obtener(
                int CodEmpresa)
        {
            return _db
                .CC_Provincias_Obtener(
                    CodEmpresa);
        }

        public List<CCGenericList>
            CC_Cantones_Obtener(
                int CodEmpresa,
                int Provincia)
        {
            return _db
                .CC_Cantones_Obtener(
                    CodEmpresa,
                    Provincia);
        }

        public List<CCGenericList>
            CC_Distritos_Obtener(
                int CodEmpresa,
                int Provincia,
                int Canton)
        {
            return _db
                .CC_Distritos_Obtener(
                    CodEmpresa,
                    Provincia,
                    Canton);
        }

        public List<CCGenericList>
            CC_Catalogo_Obtener(
                int CodEmpresa)
        {
            return _db
                .CC_Catalogo_Obtener(
                    CodEmpresa);
        }

        public List<CCGenericList>
            CC_Catalogo_Destinos_Obtener(
                int CodEmpresa,
                string? CodCatalgo)
        {
            return _db
                .CC_Catalogo_Destinos_Obtener(
                    CodEmpresa,
                    CodCatalgo);
        }

        public List<CCGenericList>
            CC_Catalogo_Grupos_Obtener(
                int CodEmpresa,
                string? CodCatalgo)
        {
            return _db
                .CC_Catalogo_Grupos_Obtener(
                    CodEmpresa,
                    CodCatalgo);
        }

        public List<CCGenericList>
            CC_Departamentos_Obtener(
                int CodEmpresa,
                string? CodInstitucion)
        {
            return _db
                .CC_Departamentos_Obtener(
                    CodEmpresa,
                    CodInstitucion);
        }

        public List<CCGenericList>
            CC_Secciones_Obtener(
                int CodEmpresa,
                string? CodInstitucion,
                string? CodDepartamento)
        {
            return _db
                .CC_Secciones_Obtener(
                    CodEmpresa,
                    CodInstitucion,
                    CodDepartamento);
        }

        public List<CbrAnalisisCubosData>
            CbrAnalisis_Cubos_SP(
                int CodEmpresa,
                string nombreSP,
                int Anio,
                int Mes)
        {
            return _db
                .CbrAnalisis_Cubos_SP(
                    CodEmpresa,
                    nombreSP,
                    Anio,
                    Mes);
        }

        public List<CbrEstimacionData>
            CbrEstimacion_SP(
                int CodEmpresa,
                string nombreSP,
                int Anio,
                int Mes)
        {
            return _db
                .CbrEstimacion_SP(
                    CodEmpresa,
                    nombreSP,
                    Anio,
                    Mes);
        }
    }
}