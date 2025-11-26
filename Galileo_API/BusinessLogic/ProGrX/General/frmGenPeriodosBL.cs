using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.GEN;

namespace Galileo.BusinessLogic
{
    public class FrmGenPeriodosBl
    {
        readonly FrmGenPeriodosDb _db;
        public FrmGenPeriodosBl(IConfiguration config)
        {
            _db = new FrmGenPeriodosDb(config);
        }
        public ErrorDto<List<PeriodoDto>> Periodos_ObtenerTodos(int CodEmpresa, string estado)
        {
            return _db.Periodos_ObtenerTodos(CodEmpresa, estado);
        }

        public ErrorDto Periodo_Cerrar(int CodEmpresa, PeriodoDto periodoDto)
        {
            return _db.Periodo_Cerrar(CodEmpresa, periodoDto);
        }

    }
}
