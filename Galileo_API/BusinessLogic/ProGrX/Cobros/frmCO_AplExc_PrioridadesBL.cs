using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cobros;
using Galileo_API.DataBaseTier.ProGrX.Cobros;

namespace Galileo_API.BusinessLogic.ProGrX.Cobros
{
    public class FrmCOAplExcPrioridadesBL
    {
        private readonly FrmCOAplExcPrioridadesDB _db;

        public FrmCOAplExcPrioridadesBL(IConfiguration config)
        {
            _db = new FrmCOAplExcPrioridadesDB(config);
        }

        public ErrorDto<COAplExcPrioridadesListaResult> Co_AplExc_Prioridades_Lista_Obtener(int CodEmpresa, string jfiltros)
        {
            return _db.Co_AplExc_Prioridades_Lista_Obtener(CodEmpresa, jfiltros);
        }

        public ErrorDto<COAplExcPrioridadesListaResult> Co_AplExc_Prioridades_Lista_Export(int CodEmpresa, string jfiltros)
        {
            return _db.Co_AplExc_Prioridades_Lista_Export(CodEmpresa, jfiltros);
        }

        public ErrorDto Co_AplExc_Prioridades_Guardar(int CodEmpresa, string usuario, COAplExcPrioridadData prioridad)
        {
            return _db.Co_AplExc_Prioridades_Guardar(CodEmpresa, usuario, prioridad);
        }

        public ErrorDto Co_AplExc_Prioridades_Eliminar(int CodEmpresa, string usuario, string codigo)
        {
            return _db.Co_AplExc_Prioridades_Eliminar(CodEmpresa, usuario, codigo);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Co_AplExc_Prioridades_GarantiasDisponibles_Obtener(int CodEmpresa)
        {
            return _db.Co_AplExc_Prioridades_GarantiasDisponibles_Obtener(CodEmpresa);
        }
    }
}
