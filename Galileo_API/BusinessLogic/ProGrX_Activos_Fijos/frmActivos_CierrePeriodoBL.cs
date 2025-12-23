using Galileo.DataBaseTier.ProGrX_Activos_Fijos;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic.ProGrX_Activos_Fijos
{
    public class FrmActivosCierrePeriodoBL
    {
        private readonly FrmActivosCierrePeriodoDb _db;

        public FrmActivosCierrePeriodoBL(IConfiguration config)
        {
            _db = new FrmActivosCierrePeriodoDb(config);
        }
        public ErrorDto<string?> Activos_PeriodoEstado_Obtener(int CodEmpresa, DateTime periodo)
        {
            return _db.Activos_PeriodoEstado_Obtener(CodEmpresa, periodo);
        }
        public ErrorDto Activos_Periodo_Cerrar(int CodEmpresa, string usuario, DateTime periodo)
        {
            return _db.Activos_Periodo_Cerrar(CodEmpresa, usuario, periodo);
        }
        public ErrorDto<DateTime> Activos_Periodo_Consultar(int CodEmpresa, int contabilidad)
        {
            return _db.Activos_Periodo_Consultar(CodEmpresa, contabilidad);
        }
    }
}
