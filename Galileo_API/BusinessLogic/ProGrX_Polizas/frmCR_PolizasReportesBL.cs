using Galileo.Models.ERROR;
using Galileo.Models;
using Galileo_API.DataBaseTier.ProGrX_Polizas;
using static Galileo_API.Models.ProGrX_Polizas.frmCR_PolizasReportesModels;

namespace Galileo_API.BusinessLogic.ProGrX_Polizas
{
    public class FrmCRPolizasReportesBL
    {
        private readonly FrmCRPolizasReportesDB _db;

        public FrmCRPolizasReportesBL(IConfiguration config)
        {
            _db = new FrmCRPolizasReportesDB(config);
        }

        public ErrorDto<List<CrdPolizasLineaModel>> Cr_PolizasReportes_Lineas_Obtener(int codEmpresa)
             => _db.Cr_PolizasReportes_Lineas_Obtener(codEmpresa);
        public ErrorDto<List<DropDownListaGenericaModel>> Cr_PolizasReportes_Departamentos_Obtene(int codEmpresa, string usuario, int codContabilidad)
              => _db.Cr_PolizasReportes_Departamentos_Obtene(codEmpresa, usuario, codContabilidad);
        public ErrorDto<List<DropDownListaGenericaModel>> Crd_PolizasReportes_Secciones_Obtener(int codEmpresa, string usuario, int codContabilidad, string? departamentoCodigo)
            => _db.Crd_PolizasReportes_Secciones_Obtener(codEmpresa, usuario, codContabilidad, departamentoCodigo);
        public ErrorDto<List<CrdPolizasReportesSocioModel>> Cr_PolizasReportes_Socios_Obtener(int codEmpresa)
               => _db.Cr_PolizasReportes_Socios_Obtener(codEmpresa);
        public ErrorDto<List<DropDownListaGenericaModel>> Crd_PolizasReportes_Cantones_Obtener(int codEmpresa, string provincia)
               => _db.Crd_PolizasReportes_Cantones_Obtener(codEmpresa, provincia);
        public ErrorDto<List<DropDownListaGenericaModel>> Crd_PolizasReportes_Distritos_Obtener(int codEmpresa, string provincia, string canton)
               => _db.Crd_PolizasReportes_Distritos_Obtener(codEmpresa, provincia, canton);
        public ErrorDto<CrdPolizasReportesInicializarResponse> Cr_PolizasReportes_Inicializar(int codEmpresa, string usuario, int codContabilidad)
                => _db.Cr_PolizasReportes_Inicializar(codEmpresa, usuario, codContabilidad);
        public ErrorDto<CrdPolizasReporteConfigResponse> Crd_PolizasReportes_ReporteConfig_Obtener(int codEmpresa, CrdPolizasReportesRequest request, string usuario, string nombreEmpresa)
              => _db.Crd_PolizasReportes_ReporteConfig_Obtener(codEmpresa, request, usuario, nombreEmpresa);
    }
}
