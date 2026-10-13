using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.GEN;
using Microsoft.AspNetCore.Mvc;

namespace Galileo.Controllers
{
    [Route("api/frmCC_ReportesAlCorte")]
    [ApiController]
    public class FrmCcReportesAlCorteController
        : ControllerBase
    {
        private readonly FrmCcReportesAlCorteBl _bl;

        public FrmCcReportesAlCorteController(
            IConfiguration config)
        {
            _bl =
                new FrmCcReportesAlCorteBl(
                    config);
        }

        [HttpGet(
            "CC_ReportesAlCorte_Inicializar")]
        public ErrorDto<CcReportesAlCorteInicializacionData>
            CC_ReportesAlCorte_Inicializar(
                [FromQuery] int codEmpresa)
        {
            return _bl
                .CC_ReportesAlCorte_Inicializar(
                    codEmpresa);
        }

        [HttpGet(
            "CC_ReportesAlCorte_AnalisisCubos_Obtener")]
        public ErrorDto<List<CbrAnalisisCubosData>>
            CC_ReportesAlCorte_AnalisisCubos_Obtener(
                [FromQuery] int codEmpresa,
                [FromQuery]
                CcReportesAlCorteProcesoRequest request)
        {
            return _bl
                .CC_ReportesAlCorte_AnalisisCubos_Obtener(
                    codEmpresa,
                    request);
        }

        [HttpGet(
            "CC_ReportesAlCorte_Estimacion_Obtener")]
        public ErrorDto<List<CbrEstimacionData>>
            CC_ReportesAlCorte_Estimacion_Obtener(
                [FromQuery] int codEmpresa,
                [FromQuery]
                CcReportesAlCorteProcesoRequest request)
        {
            return _bl
                .CC_ReportesAlCorte_Estimacion_Obtener(
                    codEmpresa,
                    request);
        }

        [HttpGet("CC_Periodos_Obtener")]
        public List<CCGenericList>
            CC_Periodos_Obtener(
                [FromQuery] int CodEmpresa)
        {
            return _bl
                .CC_Periodos_Obtener(
                    CodEmpresa);
        }

        [HttpGet("CC_Instituciones_Obtener")]
        public List<CCGenericList>
            CC_Instituciones_Obtener(
                [FromQuery] int CodEmpresa)
        {
            return _bl
                .CC_Instituciones_Obtener(
                    CodEmpresa);
        }

        [HttpGet("CC_Profesiones_Obtener")]
        public List<CCGenericList>
            CC_Profesiones_Obtener(
                [FromQuery] int CodEmpresa)
        {
            return _bl
                .CC_Profesiones_Obtener(
                    CodEmpresa);
        }

        [HttpGet("CC_Sectores_Obtener")]
        public List<CCGenericList>
            CC_Sectores_Obtener(
                [FromQuery] int CodEmpresa)
        {
            return _bl
                .CC_Sectores_Obtener(
                    CodEmpresa);
        }

        [HttpGet("CC_Zonas_Obtener")]
        public List<CCGenericList>
            CC_Zonas_Obtener(
                [FromQuery] int CodEmpresa)
        {
            return _bl
                .CC_Zonas_Obtener(
                    CodEmpresa);
        }

        [HttpGet("CC_Estados_Persona_Obtener")]
        public List<CCGenericList>
            CC_Estados_Persona_Obtener(
                [FromQuery] int CodEmpresa)
        {
            return _bl
                .CC_Estados_Persona_Obtener(
                    CodEmpresa);
        }

        [HttpGet("CC_Garantias_Obtener")]
        public List<CCGenericList>
            CC_Garantias_Obtener(
                [FromQuery] int CodEmpresa)
        {
            return _bl
                .CC_Garantias_Obtener(
                    CodEmpresa);
        }

        [HttpGet("CC_Carteras_Obtener")]
        public List<CCGenericList>
            CC_Carteras_Obtener(
                [FromQuery] int CodEmpresa)
        {
            return _bl
                .CC_Carteras_Obtener(
                    CodEmpresa);
        }

        [HttpGet("CC_Oficinas_Obtener")]
        public List<CCGenericList>
            CC_Oficinas_Obtener(
                [FromQuery] int CodEmpresa)
        {
            return _bl
                .CC_Oficinas_Obtener(
                    CodEmpresa);
        }

        [HttpGet("CC_Estados_Civiles_Obtener")]
        public List<CCGenericList>
            CC_Estados_Civiles_Obtener(
                [FromQuery] int CodEmpresa)
        {
            return _bl
                .CC_Estados_Civiles_Obtener(
                    CodEmpresa);
        }

        [HttpGet("CC_Estados_Laborales_Obtener")]
        public List<CCGenericList>
            CC_Estados_Laborales_Obtener(
                [FromQuery] int CodEmpresa)
        {
            return _bl
                .CC_Estados_Laborales_Obtener(
                    CodEmpresa);
        }

        [HttpGet("CC_Provincias_Obtener")]
        public List<CCGenericList>
            CC_Provincias_Obtener(
                [FromQuery] int CodEmpresa)
        {
            return _bl
                .CC_Provincias_Obtener(
                    CodEmpresa);
        }

        [HttpGet("CC_Cantones_Obtener")]
        public List<CCGenericList>
            CC_Cantones_Obtener(
                [FromQuery] int CodEmpresa,
                [FromQuery] int Provincia)
        {
            return _bl
                .CC_Cantones_Obtener(
                    CodEmpresa,
                    Provincia);
        }

        [HttpGet("CC_Distritos_Obtener")]
        public List<CCGenericList>
            CC_Distritos_Obtener(
                [FromQuery] int CodEmpresa,
                [FromQuery] int Provincia,
                [FromQuery] int Canton)
        {
            return _bl
                .CC_Distritos_Obtener(
                    CodEmpresa,
                    Provincia,
                    Canton);
        }

        [HttpGet("CC_Catalogo_Obtener")]
        public List<CCGenericList>
            CC_Catalogo_Obtener(
                [FromQuery] int CodEmpresa)
        {
            return _bl
                .CC_Catalogo_Obtener(
                    CodEmpresa);
        }

        [HttpGet("CC_Catalogo_Destinos_Obtener")]
        public List<CCGenericList>
            CC_Catalogo_Destinos_Obtener(
                [FromQuery] int CodEmpresa,
                [FromQuery] string? CodCatalgo)
        {
            return _bl
                .CC_Catalogo_Destinos_Obtener(
                    CodEmpresa,
                    CodCatalgo);
        }

        [HttpGet("CC_Catalogo_Grupos_Obtener")]
        public List<CCGenericList>
            CC_Catalogo_Grupos_Obtener(
                [FromQuery] int CodEmpresa,
                [FromQuery] string? CodCatalgo)
        {
            return _bl
                .CC_Catalogo_Grupos_Obtener(
                    CodEmpresa,
                    CodCatalgo);
        }

        [HttpGet("CC_Departamentos_Obtener")]
        public List<CCGenericList>
            CC_Departamentos_Obtener(
                [FromQuery] int CodEmpresa,
                [FromQuery] string? CodInstitucion)
        {
            return _bl
                .CC_Departamentos_Obtener(
                    CodEmpresa,
                    CodInstitucion);
        }

        [HttpGet("CC_Secciones_Obtener")]
        public List<CCGenericList>
            CC_Secciones_Obtener(
                [FromQuery] int CodEmpresa,
                [FromQuery] string? CodInstitucion,
                [FromQuery] string? CodDepartamento)
        {
            return _bl
                .CC_Secciones_Obtener(
                    CodEmpresa,
                    CodInstitucion,
                    CodDepartamento);
        }

        [HttpGet("CbrAnalisis_Cubos_SP")]
        public List<CbrAnalisisCubosData>
            CbrAnalisis_Cubos_SP(
                [FromQuery] int CodEmpresa,
                [FromQuery] string nombreSP,
                [FromQuery] int Anio,
                [FromQuery] int Mes)
        {
            return _bl
                .CbrAnalisis_Cubos_SP(
                    CodEmpresa,
                    nombreSP,
                    Anio,
                    Mes);
        }

        [HttpGet("CbrEstimacion_SP")]
        public List<CbrEstimacionData>
            CbrEstimacion_SP(
                [FromQuery] int CodEmpresa,
                [FromQuery] string nombreSP,
                [FromQuery] int Anio,
                [FromQuery] int Mes)
        {
            return _bl
                .CbrEstimacion_SP(
                    CodEmpresa,
                    nombreSP,
                    Anio,
                    Mes);
        }
    }
}