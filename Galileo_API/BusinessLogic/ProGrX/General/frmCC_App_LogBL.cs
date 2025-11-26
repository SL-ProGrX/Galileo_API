using Galileo.DataBaseTier;
using Galileo.Models.GEN;

namespace Galileo.BusinessLogic
{
    public class FrmCcAppLogBl
    {
        readonly FrmCcAppLogDb App_LogDb;

        public FrmCcAppLogBl(IConfiguration config)
        {
            App_LogDb = new FrmCcAppLogDb(config);
        }
        public List<EstadisticaData> CC_Estadistica_SP(int CodEmpresa, string FechaInicio, string FechaCorte)
        {
            return App_LogDb.CC_Estadistica_SP(CodEmpresa, FechaInicio, FechaCorte);
        }

        public List<EstadisticaDetalleData> CC_Estadistica_Detalle_SP(int CodEmpresa, string Codigo, string FechaInicio, string FechaCorte)
        {
            return App_LogDb.CC_Estadistica_Detalle_SP(CodEmpresa, Codigo, FechaInicio, FechaCorte);
        }

        public List<EstadisticaAnalisisData> CC_Estadistica_Analisis_SP(int CodEmpresa, string FechaInicio, string FechaCorte, int Ingreso)
        {
            return App_LogDb.CC_Estadistica_Analisis_SP(CodEmpresa, FechaInicio, FechaCorte, Ingreso);
        }
    }
}