using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar;
using Galileo_API.Models.ProGrX.CuentasxCobrar;
using Newtonsoft.Json;


namespace Galileo_API.BusinessLogic.ProGrX.CuentasxCobrar
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
