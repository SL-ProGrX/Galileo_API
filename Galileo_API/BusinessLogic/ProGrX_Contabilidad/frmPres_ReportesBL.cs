using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.PRES;

namespace Galileo.BusinessLogic
{
    public class FrmPresReportesBl
    {
        readonly FrmPresReportesDb DbfrmPres_Reportes;

        public FrmPresReportesBl(IConfiguration config)
        {
            DbfrmPres_Reportes = new FrmPresReportesDb(config);
        }

        public ErrorDto<List<ModeloGenericList>> fxPres_Periodo_Obtener(int CodEmpresa, int CodContab, string CodModelo)
        {
            return DbfrmPres_Reportes.fxPres_Periodo_Obtener(CodEmpresa, CodContab, CodModelo);
        }

        public ErrorDto<List<ModeloGenericList>> spPres_Ajustes_Permitidos_Obtener(int CodEmpresa, int codContab, string codModelo, string Usuario)
        {
            return DbfrmPres_Reportes.spPres_Ajustes_Permitidos_Obtener(CodEmpresa, codContab, codModelo, Usuario);
        }
    }
}