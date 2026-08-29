using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.GEN;

namespace Galileo.BusinessLogic
{
    public class FrmCcAppLogBl
    {
        private readonly FrmCcAppLogDb _db;

        public FrmCcAppLogBl(
            IConfiguration config)
        {
            _db = new FrmCcAppLogDb(config);
        }

        public ErrorDto<List<EstadisticaData>>
            CC_App_Log_Estadistica_Obtener(
                int CodEmpresa,
                string FechaInicio,
                string FechaCorte)
        {
            return _db
                .CC_App_Log_Estadistica_Obtener(
                    CodEmpresa,
                    FechaInicio,
                    FechaCorte);
        }

        public ErrorDto<List<EstadisticaDetalleData>>
            CC_App_Log_Estadistica_Detalle_Obtener(
                int CodEmpresa,
                string Codigo,
                string FechaInicio,
                string FechaCorte)
        {
            return _db
                .CC_App_Log_Estadistica_Detalle_Obtener(
                    CodEmpresa,
                    Codigo,
                    FechaInicio,
                    FechaCorte);
        }

        public ErrorDto<List<EstadisticaAnalisisData>>
            CC_App_Log_Estadistica_Analisis_Obtener(
                int CodEmpresa,
                string FechaInicio,
                string FechaCorte,
                int Ingreso)
        {
            return _db
                .CC_App_Log_Estadistica_Analisis_Obtener(
                    CodEmpresa,
                    FechaInicio,
                    FechaCorte,
                    Ingreso);
        }
    }
}