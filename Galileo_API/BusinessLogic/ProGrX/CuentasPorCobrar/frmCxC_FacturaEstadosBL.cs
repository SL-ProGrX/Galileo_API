using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.CuentasPorCobrar;
using Galileo_API.Models.ProGrX.CuentasPorCobrar;
using Newtonsoft.Json;


namespace PgxAPI.BusinessLogic.ProGrX.Cobros
{
    public class FrmCxCFacturaEstadosBL
    {
         
        private readonly FrmCxCFacturaEstadosDB _db;

        public FrmCxCFacturaEstadosBL(IConfiguration config) => _db = new FrmCxCFacturaEstadosDB(config);

        public ErrorDto<CxCFacturaEstadosLista> CxCFacturaEstadosLista_Obtener(int codEmpresa, string jfiltros, bool esExportar)
        {

            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.CxCFacturaEstadosLista_Obtener(codEmpresa, filtros, esExportar);
        }
        public ErrorDto CxCFacturaEstados_Guardar(int codEmpresa, string usuario, CxCFacturaEstadosData datos)
        {
            return _db.CxCFacturaEstados_Guardar(codEmpresa, usuario, datos);
        }
        public ErrorDto CxCFacturaEstados_Eliminar(int codEmpresa, string usuario, string codFactura)
        {
            return _db.CxCFacturaEstados_Eliminar(codEmpresa, usuario, codFactura);
        }
        
    }
}
