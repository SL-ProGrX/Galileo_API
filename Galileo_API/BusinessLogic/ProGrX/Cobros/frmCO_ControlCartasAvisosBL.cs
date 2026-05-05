using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cobros;
using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX.Cobros;

namespace Galileo.BusinessLogic.ProGrX.Cobros
{
    public class FrmCoControlCartasAvisosBL
    {
         
        private readonly FrmCoControlCartasAvisosDB _db;

        public FrmCoControlCartasAvisosBL(IConfiguration config) => _db = new FrmCoControlCartasAvisosDB(config);


        public ErrorDto<CoControlCartasAvisosLista> CO_ControlCartasAvisos_Buscar(int CodEmpresa, bool esExportar, string jfiltros, string cedula)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.CO_ControlCartasAvisos_Buscar(CodEmpresa, esExportar, filtros, cedula);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> CO_ControlCartasAvisos_Usuarios_Consultar(int CodEmpresa)
        {
            return _db.CO_ControlCartasAvisos_Usuarios_Consultar(CodEmpresa);
        }


    }
}
