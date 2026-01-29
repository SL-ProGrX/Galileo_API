using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar;
using Galileo_API.Models.ProGrX.CuentasxCobrar;
using Newtonsoft.Json;


namespace Galileo_API.BusinessLogic.ProGrX.CuentasxCobrar
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
