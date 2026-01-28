using Galileo.Models;
using Galileo.Models.ERROR; 
using Galileo_API.DataBaseTier.ProGrX.CuentasPorCobrar;
using Galileo_API.Models.ProGrX.CuentasPorCobrar;
using Newtonsoft.Json;


namespace PgxAPI.BusinessLogic.ProGrX.Cobros
{
    public class FrmCxCClientesClasificaBL
    {
         
        private readonly FrmCxCClientesClasificaDB _db;

        public FrmCxCClientesClasificaBL(IConfiguration config) => _db = new FrmCxCClientesClasificaDB(config);



        public ErrorDto<CxCClientesClasificaLista> CxCClientesClasificaLista_Obtener(int CodEmpresa, string jfiltros, bool esExportar)
        {

            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.CxCClientesClasificaLista_Obtener(CodEmpresa, filtros, esExportar);
        }
        public ErrorDto CxCClientesClasifica_Guardar(int CodEmpresa, string usuario, CxCClientesClasificaData datos)
        {
            return _db.CxCClientesClasifica_Guardar(CodEmpresa, usuario, datos);
        }
        public ErrorDto CxCClientesClasifica_Eliminar(int CodEmpresa, string usuario, string CodCategoria)
        {
            return _db.CxCClientesClasifica_Eliminar(CodEmpresa, usuario, CodCategoria);
        }
        
    }
}
