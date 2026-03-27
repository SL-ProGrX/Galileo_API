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

        public ErrorDto<CxCCuentasFacturasLista> CxCCuentasFacturas_Obtener(int codEmpresa, long operacion)
        {
            return _db.CxCCuentasFacturas_Obtener(codEmpresa, operacion);
        }

        public ErrorDto<CxCCuentasFacturasAdelantadasLista> CxCCuentasFacturasAdelantadas_Obtener(
            int codEmpresa,
            string cedula,
            string cedulaPagador)
        {
            return _db.CxCCuentasFacturasAdelantadas_Obtener(codEmpresa, cedula, cedulaPagador);
        }

        public ErrorDto<CxCCuentasPersonasFiltroLista> CxCCuentasPersonasFiltro_Obtener(
            int codEmpresa,
            string jfiltros,
            bool esExportar)
        {
            FiltrosLazyLoadData filtros =
                JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();

            return _db.CxCCuentasPersonasFiltro_Obtener(codEmpresa, filtros, esExportar);
        }

        public ErrorDto<CxCCuentasPersonasFiltroItem> CxCCuentasPersonaFiltroPorCedula_Obtener(int codEmpresa, string cedula)
        {
            return _db.CxCCuentasPersonaFiltroPorCedula_Obtener(codEmpresa, cedula);
        }

        public ErrorDto<CxCCuentasConceptoData> CxCCuentasConcepto_Obtener(int codEmpresa, string codConcepto)
        {
            return _db.CxCCuentasConcepto_Obtener(codEmpresa, codConcepto);
        }

        public ErrorDto<CxCCuentasConceptosFiltroItem> CxCCuentasConceptoScroll_Obtener(int codEmpresa, string codConcepto, int tipo)
        {
            return _db.CxCCuentasConceptoScroll_Obtener(codEmpresa, codConcepto, tipo);
        }

        public ErrorDto<CxCCuentasBusquedaGenericaLista<CxCCuentasConceptosFiltroItem>> CxCCuentasConceptosFiltro_Obtener(
            int codEmpresa,
            string jfiltros,
            bool esExportar)
        {
            FiltrosLazyLoadData filtros =
                JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();

            return _db.CxCCuentasConceptosFiltro_Obtener(codEmpresa, filtros, esExportar);
        }

        public ErrorDto<CxCCuentasContratoData> CxCCuentasContratoDetalle_Obtener(int codEmpresa, string codContrato, string cedula)
        {
            return _db.CxCCuentasContratoDetalle_Obtener(codEmpresa, codContrato, cedula);
        }

        public ErrorDto<CxCCuentasContratosFiltroItem> CxCCuentasContratoScroll_Obtener(
            int codEmpresa,
            string cedula,
            string codConcepto,
            string codContrato,
            int tipo)
        {
            return _db.CxCCuentasContratoScroll_Obtener(codEmpresa, cedula, codConcepto, codContrato, tipo);
        }

        public ErrorDto<CxCCuentasBusquedaGenericaLista<CxCCuentasContratosFiltroItem>> CxCCuentasContratosFiltro_Obtener(
            int codEmpresa,
            string cedula,
            string codConcepto,
            string jfiltros,
            bool esExportar)
        {
            FiltrosLazyLoadData filtros =
                JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();

            return _db.CxCCuentasContratosFiltro_Obtener(codEmpresa, cedula, codConcepto, filtros, esExportar);
        }

        public ErrorDto<CxCCuentasPagadorData> CxCCuentasPagador_Obtener(
    int codEmpresa,
    string cedulaCliente,
    string codContrato,
    string cedulaPagador)
        {
            return _db.CxCCuentasPagador_Obtener(codEmpresa, cedulaCliente, codContrato, cedulaPagador);
        }

        public ErrorDto<CxCCuentasPagadoresFiltroItem> CxCCuentasPagadorScroll_Obtener(
            int codEmpresa,
            string cedulaCliente,
            string codContrato,
            string cedulaPagador,
            int tipo)
        {
            return _db.CxCCuentasPagadorScroll_Obtener(codEmpresa, cedulaCliente, codContrato, cedulaPagador, tipo);
        }

        public ErrorDto<CxCCuentasBusquedaGenericaLista<CxCCuentasPagadoresFiltroItem>> CxCCuentasPagadoresFiltro_Obtener(
            int codEmpresa,
            string cedulaCliente,
            string codContrato,
            string jfiltros,
            bool esExportar)
        {
            FiltrosLazyLoadData filtros =
                JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();

            return _db.CxCCuentasPagadoresFiltro_Obtener(codEmpresa, cedulaCliente, codContrato, filtros, esExportar);
        }

        public ErrorDto<CxCCuentasAutorizadoData> CxCCuentasAutorizado_Obtener(
            int codEmpresa,
            string cedulaCliente,
            string cedulaAutorizado)
        {
            return _db.CxCCuentasAutorizado_Obtener(codEmpresa, cedulaCliente, cedulaAutorizado);
        }

        public ErrorDto<CxCCuentasAutorizadosFiltroItem> CxCCuentasAutorizadoScroll_Obtener(
            int codEmpresa,
            string cedulaCliente,
            string cedulaAutorizado,
            int tipo)
        {
            return _db.CxCCuentasAutorizadoScroll_Obtener(codEmpresa, cedulaCliente, cedulaAutorizado, tipo);
        }

        public ErrorDto<CxCCuentasBusquedaGenericaLista<CxCCuentasAutorizadosFiltroItem>> CxCCuentasAutorizadosFiltro_Obtener(
            int codEmpresa,
            string cedulaCliente,
            string jfiltros,
            bool esExportar)
        {
            FiltrosLazyLoadData filtros =
                JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();

            return _db.CxCCuentasAutorizadosFiltro_Obtener(codEmpresa, cedulaCliente, filtros, esExportar);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CxCCuentasCuentasBancarias_Obtener(int codEmpresa, string cedula, string banco)
        {
            return _db.CxCCuentasCuentasBancarias_Obtener(codEmpresa, cedula, banco);
        }

        public ErrorDto<CxCCuentasFacturaMantenimientoResult> CxCCuentasFactura_Registra(
                int codEmpresa,
                CxCCuentasFacturaRegistraRequest request)
        {
            return _db.CxCCuentasFactura_Registra(codEmpresa, request);
        }

        public ErrorDto<CxCCuentasFacturaMantenimientoResult> CxCCuentasFactura_Elimina(
            int codEmpresa,
            CxCCuentasFacturaEliminaRequest request)
        {
            return _db.CxCCuentasFactura_Elimina(codEmpresa, request);
        }

        public ErrorDto<CxCCuentasFacturaMantenimientoResult> CxCCuentasFactura_Vincular(
            int codEmpresa,
            CxCCuentasFacturaVincularRequest request)
        {
            return _db.CxCCuentasFactura_Vincular(codEmpresa, request);
        }

        public ErrorDto<CxCCuentasFacturaMantenimientoResult> CxCCuentasFactura_CargarArchivo(
            int codEmpresa,
            CxCCuentasFacturaCargaRequest request)
        {
            return _db.CxCCuentasFactura_CargarArchivo(codEmpresa, request);
        }

        public ErrorDto<long> CxCCuentas_Guardar(int codEmpresa, CxCCuentasSaveParams param)
        {
            return _db.CxCCuentas_Guardar(codEmpresa, param);
        }

        public ErrorDto<CxCCuentasActivacionVerificaResult> CxCCuentasActivacion_Verifica(
    int codEmpresa,
    CxCCuentasActivacionRequest request)
        {
            return _db.CxCCuentasActivacion_Verifica(codEmpresa, request);
        }

        public ErrorDto<bool> CxCCuentasActivacion_Activar(
            int codEmpresa,
            CxCCuentasActivacionRequest request)
        {
            return _db.CxCCuentasActivacion_Activar(codEmpresa, request);
        }

        public ErrorDto<CxCCuentasAnulacionVerificaResult> CxCCuentasAnulacion_Verifica(
    int codEmpresa,
    CxCCuentasAnulacionRequest request)
        {
            return _db.CxCCuentasAnulacion_Verifica(codEmpresa, request);
        }

        public ErrorDto<bool> CxCCuentasAnulacion_Anular(
            int codEmpresa,
            CxCCuentasAnulacionRequest request)
        {
            return _db.CxCCuentasAnulacion_Anular(codEmpresa, request);
        }

        public ErrorDto<CxCCuentasActivacionDetalleResult> CxCCuentasActivacionDetalle_Obtener(
                int codEmpresa,
                CxCCuentasActivacionDetalleRequest request)
        {
            return _db.CxCCuentasActivacionDetalle_Obtener(codEmpresa, request);
        }
    }
}
