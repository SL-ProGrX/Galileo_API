using Galileo.DataBaseTier.ProGrX_Activos_Fijos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;

namespace Galileo.BusinessLogic.ProGrX_Activos_Fijos
{
    public class FrmActivosPolizasReportesBL
    {
        private readonly FrmActivosPolizasReportesDB _db;

        public FrmActivosPolizasReportesBL(IConfiguration config)
        {
            _db = new FrmActivosPolizasReportesDB(config);
        }
        
        public ErrorDto<ActivosPolizasReportesLista> Activos_PolizasReportesLista_Obtener(int CodEmpresa, string jfiltros)
        {
            return _db.Activos_PolizasReportesLista_Obtener(CodEmpresa, jfiltros);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Activos_PolizasReportes_Tipos_Lista_Obtener(int CodEmpresa)
        {
            return _db.Activos_PolizasReportes_Tipos_Lista_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Activos_PolizasReportes_Estados_Lista_Obtener(int CodEmpresa)
        {
            return _db.Activos_PolizasReportes_Estados_Lista_Obtener(CodEmpresa);
        }
    }
}
