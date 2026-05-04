using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Cobros;
using static Galileo_API.Models.ProGrX.Cobros.FrmCoAplFndPrioridadesModels;

namespace Galileo_API.BusinessLogic.ProGrX.Cobros
{
    public class FrmCoAplFndPrioridadesBl
    {
        private readonly FrmCoAplFndPrioridadesDb _db;

        public FrmCoAplFndPrioridadesBl(IConfiguration config)
        {
            _db = new FrmCoAplFndPrioridadesDb(config);
        }

        public ErrorDto<COAplFndPrioridadesListaResult> Co_AplFnd_Prioridades_Lista_Obtener(int CodEmpresa, string jfiltros)
        {
            return _db.Co_AplFnd_Prioridades_Lista_Obtener(CodEmpresa, jfiltros);
        }

        public ErrorDto<COAplFndPrioridadesListaResult> Co_AplFnd_Prioridades_Lista_Export(int CodEmpresa, string jfiltros)
        {
            return _db.Co_AplFnd_Prioridades_Lista_Export(CodEmpresa, jfiltros);
        }

        public ErrorDto Co_AplFnd_Prioridades_Guardar(int CodEmpresa, string usuario, COAplFndPrioridadData prioridad)
        {
            return _db.Co_AplFnd_Prioridades_Guardar(CodEmpresa, usuario, prioridad);
        }

        public ErrorDto Co_AplFnd_Prioridades_Eliminar(int CodEmpresa, string usuario, string codigo)
        {
            return _db.Co_AplFnd_Prioridades_Eliminar(CodEmpresa, usuario, codigo);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Co_AplFnd_Prioridades_GarantiasDisponibles_Obtener(int CodEmpresa)
        {
            return _db.Co_AplFnd_Prioridades_GarantiasDisponibles_Obtener(CodEmpresa);
        }

        public ErrorDto<int> Co_AplFnd_PrioridadEjecucion_Obtener(int CodEmpresa)
        {
            return _db.Co_AplFnd_PrioridadEjecucion_Obtener(CodEmpresa);
        }

        public ErrorDto Co_AplFnd_PrioridadEjecucion_Actualizar(int CodEmpresa, string usuario, int prioridad)
        {
            return _db.Co_AplFnd_PrioridadEjecucion_Actualizar(CodEmpresa, usuario, prioridad);
        }
    }
}
