using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Cobros;
using Galileo_API.Models.ProGrX.Cobros;

namespace Galileo_API.BusinessLogic.ProGrX.Cobros
{
    public class FrmCOAplFndPlanesBL
    {
        private readonly FrmCOAplFndPlanesDB _db;

        public FrmCOAplFndPlanesBL(IConfiguration config)
        {
            _db = new FrmCOAplFndPlanesDB(config);
        }

        public ErrorDto<List<FondosAplConfigPrioridadResult>> FondosAplConfigPrioridades_Lista(int codEmpresa)
        {
            return _db.FondosAplConfigPrioridades_Lista(codEmpresa);
        }

        public ErrorDto<List<FondosAplConfigFondoDisponibleResult>> FondosAplConfigFondosDisponibles_Lista(int codEmpresa)
        {
            return _db.FondosAplConfigFondosDisponibles_Lista(codEmpresa);
        }

        public ErrorDto<FondosAplConfigPrioridadAddResult?> FondosAplConfigPrioridad_Add(int codEmpresa, FondosAplConfigPrioridadAddParams param)
        {
            return _db.FondosAplConfigPrioridad_Add(codEmpresa, param);
        }

        public ErrorDto<FondosAplConfigPrioridadDelResult?> FondosAplConfigPrioridad_Del(int codEmpresa, FondosAplConfigPrioridadDelParams param)
        {
            return _db.FondosAplConfigPrioridad_Del(codEmpresa, param);
        }
    }
}
