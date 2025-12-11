using Galileo.DataBaseTier.ProGrX_Activos_Fijos;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;

namespace Galileo.BusinessLogic.ProGrX_Activos_Fijos
{
    public class FrmActivosPeriodosBL
    {
        private readonly FrmActivosPeriodosDB _db;

        public FrmActivosPeriodosBL(IConfiguration config)
        {
            _db = new FrmActivosPeriodosDB(config);
        }
        public ErrorDto<ActivosPeriodosDataLista> Activos_Periodos_Consultar(int CodEmpresa, string estado)
        {
            return _db.Activos_Periodos_Consultar(CodEmpresa, estado);
        }
    }
}
