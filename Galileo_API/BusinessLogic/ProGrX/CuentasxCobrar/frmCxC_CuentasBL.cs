using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar;
using Galileo_API.Models.ProGrX.CuentasxCobrar;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic.ProGrX.CuentasxCobrar
{
    public class FrmCxcCuentasBL
    {
        private readonly FrmCxcCuentasDB _db;

        public FrmCxcCuentasBL(IConfiguration config)
        {
            _db = new FrmCxcCuentasDB(config);
        }

        public DateTime fxFechaServidor(int codEmpresa)
        {
            return _db.fxFechaServidor(codEmpresa);
        }

        public ErrorDto<string> fxCxC_Parametro(int codEmpresa, string codParametro)
        {
            return _db.fxCxC_Parametro(codEmpresa, codParametro);
        }

        public ErrorDto<CxCCuentasBusquedaOperacionLista> CxCCuentasBusquedaOperacionLista_Obtener(
           int codEmpresa,
           string jfiltros,
           bool esExportar)
        {
            FiltrosLazyLoadData filtros =
                JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();

            return _db.CxCCuentasBusquedaOperacionLista_Obtener(codEmpresa, filtros, esExportar);
        }

        public ErrorDto<CxCCuentasBusquedaOperacionItem> CxCCuentasOperacion_Obtener(int codEmpresa, long operacion)
        {
            return _db.CxCCuentasOperacion_Obtener(codEmpresa, operacion);
        }

        public ErrorDto<CxCCuentasBusquedaOperacionItem> CxCCuentasOperacionScroll_Obtener(int codEmpresa, long operacion, int tipo)
        {
            return _db.CxCCuentasOperacionScroll_Obtener(codEmpresa, operacion, tipo);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CxCCuentas_Divisas_Obtener(int codEmpresa, int codContabilidad)
        {
            return _db.CxCCuentas_Divisas_Obtener(codEmpresa, codContabilidad);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CxCCuentas_Oficinas_Obtener(int codEmpresa)
        {
            return _db.CxCCuentas_Oficinas_Obtener(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CxCCuentas_Bancos_Obtener(int codEmpresa)
        {
            return _db.CxCCuentas_Bancos_Obtener(codEmpresa);
        }

        public ErrorDto<CxCCuentasConsultaData> CxCCuentas_Consulta_Obtener(int codEmpresa, long operacion)
        {
            return _db.CxCCuentas_Consulta_Obtener(codEmpresa, operacion);
        }
    }
}
