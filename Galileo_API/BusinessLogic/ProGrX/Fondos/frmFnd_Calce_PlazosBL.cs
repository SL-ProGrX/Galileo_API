using Galileo.DataBaseTier.ProGrX.Fondos;
using Galileo.Models;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic.ProGrX.Fondos
{
    public class FrmFndCalcePlazosBL
    {
        private readonly FrmFndCalcePLazosDB _Db;

        public FrmFndCalcePlazosBL(IConfiguration config)
        {
            _Db = new FrmFndCalcePLazosDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Periodos_Lista(int CodEmpresa)
        {
            return _Db.Periodos_Lista(CodEmpresa);
        }

        public ErrorDto Proyeccion_Presupuesto(int CodEmpresa, int Anio, string Usuario, int Tipo)
        {
            return _Db.Proyeccion_Presupuesto(CodEmpresa, Anio, Usuario, Tipo);
        }

        public ErrorDto<List<Dictionary<string, object?>>> Proyeccion_Presupuesto_Export(int CodEmpresa, int Anio)
        {
            return _Db.Proyeccion_Presupuesto_Export(CodEmpresa, Anio);
        }

    }
}
