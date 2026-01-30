using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cobros;
using Galileo_API.DataBaseTier.ProGrX.Cobros;


namespace Galileo_API.BusinessLogic.ProGrX.Cobros
{
    public class FrmCoAdvertenciasControlBL
    {

        private readonly FrmCoAdvertenciasControlDB _db;

        public FrmCoAdvertenciasControlBL(IConfiguration config) => _db = new FrmCoAdvertenciasControlDB(config);



        public ErrorDto<List<DropDownListaGenericaModel>> TiposAdvertiencia_Consultar(int CodEmpresa)
        {
            return _db.TiposAdvertiencia_Consultar(CodEmpresa);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> EstadosPersonas_Consultar(int CodEmpresa)
        {
            return _db.EstadosPersonas_Consultar(CodEmpresa);
        }
        public ErrorDto<CoAdvertenciasControlLista> CoAdvertenciasControlBuscar(int CodEmpresa, CoAdvertenciasControlFiltros filtros, bool esExportar)
        {
            return _db.CoAdvertenciasControlBuscar(CodEmpresa, filtros, esExportar);
        }

    }
}
