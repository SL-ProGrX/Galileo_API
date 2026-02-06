using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar;
using Galileo_API.Models.ProGrX.CuentasxCobrar;
using Newtonsoft.Json;


namespace Galileo_API.BusinessLogic.ProGrX.CuentasxCobrar
{
    public class FrmCxCParametrosBL
    {

        private readonly FrmCxCParametrosDB _db;

        public FrmCxCParametrosBL(IConfiguration config) => _db = new FrmCxCParametrosDB(config);



        public ErrorDto<CxCParametrosLista> CxCParametrosLista_Obtener(int CodEmpresa, int codContabilidad, string jfiltros, bool esExportar)
        {

            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.CxCParametrosLista_Obtener(CodEmpresa, codContabilidad,filtros, esExportar);
        }
        public ErrorDto CxCParametros_Guardar(int CodEmpresa, string usuario, string valor, string codParametro)
        {
            return _db.CxCParametros_Guardar(CodEmpresa, usuario, valor, codParametro);
        }

    }
}
