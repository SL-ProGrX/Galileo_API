using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cobros;
using Galileo_API.DataBaseTier.ProGrX.Cobros;
using Galileo_API.DataBaseTier.ProGrX.CuentasPorCobrar;
using Galileo_API.Models.ProGrX.CuentasPorCobrar;
using Newtonsoft.Json;


namespace PgxAPI.BusinessLogic.ProGrX.Cobros
{
    public class FrmCxCCargosTiposBL
    {
         
        private readonly FrmCxCCargosTiposDB _db;

        public FrmCxCCargosTiposBL(IConfiguration config) => _db = new FrmCxCCargosTiposDB(config);



        public ErrorDto<CxCCargosTiposLista> CxCCargosTiposLista_Obtener(int CodEmpresa, string jfiltros, bool esExportar)
        {

            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.CxCCargosTiposLista_Obtener(CodEmpresa, filtros, esExportar);
        }
        public ErrorDto CxCCargosTipos_Guardar(int CodEmpresa, string usuario, CxCCargosTiposData datos)
        {
            return _db.CxCCargosTipos_Guardar(CodEmpresa, usuario, datos);
        }
        public ErrorDto CxCCargosTipos_Eliminar(int CodEmpresa, string usuario, string CodCargo)
        {
            return _db.CxCCargosTipos_Eliminar(CodEmpresa, usuario, CodCargo);
        }
        
    }
}
