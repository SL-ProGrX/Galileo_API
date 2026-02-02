using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Bancos;
using Galileo_API.Models.ProGrX.Bancos;

namespace Galileo_API.BusinessLogic.ProGrX.Bancos
{
    public class FrmTesMonitorSinpeBL
    {
        private readonly FrmTesMonitorSinpeDB _db;
        public FrmTesMonitorSinpeBL(IConfiguration config)
        {
            _db = new FrmTesMonitorSinpeDB(config);
        }

        public ErrorDto<decimal> fxFnd_SobresConsultaTotal(int CodEmpresa, string? cedula, string? plan)
        {
            return _db.fxFnd_SobresConsultaTotal(CodEmpresa, cedula, plan);
        }

        public ErrorDto<decimal> Tes_MonitorSinpeContrato_Consultar(int CodEmpresa)
        {
            return _db.Tes_MonitorSinpeContrato_Consultar(CodEmpresa);
        }

        public ErrorDto<List<TesMonitorSinpeDebCrdModels>> Tes_MonitorSinpeDebCred_Consultar(int CodEmpresa, DateTime fechaInicio, DateTime fechaFin)
        {
            return _db.Tes_MonitorSinpeDebCred_Consultar(CodEmpresa, fechaInicio, fechaFin);
        }

        public ErrorDto<List<TesMonitorSinpeDebCrdModels>> Tes_MonitorSinpeTransitos_Consultar(int CodEmpresa, DateTime fechaInicio, DateTime fechaFin)
        {
            return _db.Tes_MonitorSinpeTransitos_Consultar(CodEmpresa, fechaInicio, fechaFin);
        }

    }
}
