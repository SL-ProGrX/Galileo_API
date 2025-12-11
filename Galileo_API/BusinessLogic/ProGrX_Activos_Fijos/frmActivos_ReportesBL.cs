using Galileo.DataBaseTier.ProGrX_Activos_Fijos;
using Galileo.Models.ERROR;
using Galileo.Models;
using Galileo.Models.ProGrX_Activos_Fijos;

namespace Galileo.BusinessLogic.ProGrX_Activos_Fijos
{
    public class FrmActivosReportesBL
    {
        private readonly FrmActivosReportesDB _db;

        public FrmActivosReportesBL(IConfiguration config)
        {
            _db = new FrmActivosReportesDB(config);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Reportes_Departamentos_Obtener(int CodEmpresa) { 
            return _db.Activos_Reportes_Departamentos_Obtener(CodEmpresa);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Reportes_Secciones_Obtener(int CodEmpresa, string departamento)
        {
            return _db.Activos_Reportes_Secciones_Obtener(CodEmpresa, departamento);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Reportes_TipoActivo_Obtener(int CodEmpresa)
        {
            return _db.Activos_Reportes_TipoActivo_Obtener(CodEmpresa);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Reportes_Localizacion_Obtener(int CodEmpresa)
        {
            return _db.Activos_Reportes_Localizacion_Obtener(CodEmpresa);
        }

        public ErrorDto<string> Activos_Reportes_PeriodoEstado(int CodEmpresa, DateTime fecha)
        {
            return _db.Activos_Reportes_PeriodoEstado(CodEmpresa, fecha);
        }

        public ErrorDto<DateTime> Activos_Periodo_Consultar(int CodEmpresa, int contabilidad)
        {
            return _db.Activos_Periodo_Consultar(CodEmpresa, contabilidad);
        }

        public ErrorDto<List<ActivosReportesResponsableData>> Activos_Reportes_Responsables_Consultart(int CodEmpresa)
        {
            return _db.Activos_Reportes_Responsables_Consultart(CodEmpresa);
        }

    }
}