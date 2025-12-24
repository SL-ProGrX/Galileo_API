using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.TES;
using Galileo_API.DataBaseTier.ProGrX.Bancos;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic.ProGrX.Bancos
{
    public class FrmTesTransaccionesBL
    {
        private readonly FrmTesTransaccionesDb _db;

        public FrmTesTransaccionesBL(IConfiguration config)
        {
            _db = new FrmTesTransaccionesDb(config);
        }

        public ErrorDto<TablasListaGenericaModel> TES_Solicitudes_Obtener(int CodEmpresa, int contabilidad, string filtro)
        {
            FiltrosLazyLoadData jfiltro = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(filtro);
            return _db.TES_Solicitudes_Obtener(CodEmpresa, contabilidad, jfiltro);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> TES_TiposDocumentos_Obtener(int CodEmpresa, string Usuario, int id_banco, string? tipo = "S")
        {
            return _db.TES_TiposDocumentos_Obtener(CodEmpresa, Usuario, id_banco, tipo);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> TiposIdentificacion_Obtener(int CodEmpresa)
        {
            return _db.TiposIdentificacion_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> TES_Unidades_Obtener(int CodEmpresa, string usuario, int banco, int contabilidad)
        {
            return _db.TES_Unidades_Obtener(CodEmpresa, usuario, banco, contabilidad);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> TES_Conceptos_Obtener(int CodEmpresa, string usuario, int banco)
        {
            return _db.TES_Conceptos_Obtener(CodEmpresa, usuario, banco);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> TES_BancosCarga_Obtener(int CodEmpresa, string usuario, string gestion)
        {
            return _db.TES_BancosCarga_Obtener(CodEmpresa, usuario, gestion);
        }

        public ErrorDto<int> TES_Transaccion_Scroll(int CodEmpresa, int scrollCode, string codigo, int contabilidad)
        {
            return _db.TES_Transaccion_Scroll(CodEmpresa, scrollCode, codigo, contabilidad);
        }

        public ErrorDto<TesTransaccionDto> TES_Transaccion_Obtener(int CodEmpresa, int tesoreria, int contabilidad)
        {
            return _db.TES_Transaccion_Obtener(CodEmpresa, tesoreria, contabilidad);
        }

        public ErrorDto<List<TesAfectacionDto>> TES_Afectaciones_Obtener(int CodEmpresa, int tesoreria)
        {
            return _db.TES_Afectaciones_Obtener(CodEmpresa, tesoreria);
        }

        public ErrorDto<List<TesTransAsientoDto>> TES_TransaccionAsiento_Obtener(string vSolicitud)
        {
            TesConsultaAsientos solicitud = JsonConvert.DeserializeObject<TesConsultaAsientos>(vSolicitud);
            return _db.TES_TransaccionAsiento_Obtener(solicitud);
        }

        public ErrorDto<List<TesBitacoraDto>> TES_Bitacora_Obtener(int CodEmpresa, int tesoreria)
        {
            return _db.TES_Bitacora_Obtener(CodEmpresa, tesoreria);
        }

        public ErrorDto<List<TesLocalizacionDto>> TES_Localizacion_Obtener(int CodEmpresa, int solicitud)
        {
            return _db.TES_Localizacion_Obtener(CodEmpresa, solicitud);
        }

        public ErrorDto<List<TesReimpresionesDto>> TES_ReImpresiones_Obtener(int CodEmpresa, int solicitud)
        {
            return _db.TES_ReImpresiones_Obtener(CodEmpresa, solicitud);
        }

        public ErrorDto<List<TesCambioFechasDto>> TES_CambioFechas_Obtener(int CodEmpresa, int solicitud)
        {
            return _db.TES_CambioFechas_Obtener(CodEmpresa, solicitud);
        }

        public ErrorDto<int> TES_TransaccionDocumento_Scroll(int CodEmpresa, int scrollCode, string parametros)
        {
            TesSolicitudDocParametro parametro = JsonConvert.DeserializeObject<TesSolicitudDocParametro>(parametros);
            return _db.TES_TransaccionDocumento_Scroll(CodEmpresa, scrollCode, parametro);
        }

        public ErrorDto<int> TES_TransaccionDoc_Obtener(
           int CodEmpresa,
           string documento,
           int banco,
           string tipo,
           int contabilidad)
        {
            return _db.TES_TransaccionDoc_Obtener(
               CodEmpresa,
               documento,
               banco,
               tipo,
               contabilidad);
        }

        public ErrorDto TES_CambioCuentaBancaria_Aplicar(int CodEmpresa, string usuario, int solicitud, string cuenta)
        {
            return _db.TES_CambioCuentaBancaria_Aplicar(CodEmpresa, usuario, solicitud, cuenta);
        }

        public ErrorDto TES_Transaccion_Guardar(int CodEmpresa, string usuario, int contabilidad, TesTransaccionDto transaccion)
        {
            return _db.TES_Transaccion_Guardar(CodEmpresa, usuario, contabilidad, transaccion);
        }

        public ErrorDto<TablasListaGenericaModel> TES_transaccionesBeneficiario_Obtener(
            int CodEmpresa, string tipo, string filtro)
        {
            return _db.TES_transaccionesBeneficiario_Obtener(CodEmpresa, tipo, filtro);
        }

        public ErrorDto<TesDivisaAsiento> TES_TransaccionesDivisa_Obtener(int CodEmpresa, string cuenta, int contabilidad)
        {
            return _db.TES_TransaccionesDivisa_Obtener(CodEmpresa, cuenta, contabilidad);
        }

        public ErrorDto<float> TES_TransaccionesTC_Obtener(int CodEmpresa, string cod_divisa, int contabilidad)
        {
            return _db.TES_TransaccionesTC_Obtener(CodEmpresa, cod_divisa, contabilidad);
        }

        public ErrorDto TES_TransferenciasSinpe_Valida(int CodEmpresa, int solicitud, string usuario)
        {
            return _db.TES_TransferenciasSinpe_Valida(CodEmpresa, solicitud, usuario);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> TES_TransaccionesUnidades_Obtener(int CodEmpresa, int contabilidad)
        {
            return _db.TES_TransaccionesUnidades_Obtener(CodEmpresa, contabilidad);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> TES_TransaccionesCC_Obtener(int CodEmpresa, string unidad)
        {
            return _db.TES_TransaccionesCC_Obtener(CodEmpresa, unidad);
        }

        public ErrorDto<TesControlDivisas> TesControlDivisas_Obtener(int CodEmpresa, int id_banco, int contabilidad)
        {
            return _db.TesControlDivisas_Obtener(CodEmpresa, id_banco, contabilidad);
        }

        public ErrorDto<bool> TesEmpresaSinpe_Valida(int CodEmpresa)
        {
            return _db.TesEmpresaSinpe_Valida(CodEmpresa);
        }

        public ErrorDto TES_Transacciones_Eliminar(int CodEmpresa, int solicitud, string usuario)
        {
            return _db.TES_Transacciones_Eliminar(CodEmpresa, solicitud, usuario);
        }

        public ErrorDto<List<TesBitacoraTransaccion>> Tes_BitacoraTransaccion(int CodEmpresa, string solicitud)
        {
            return _db.Tes_BitacoraTransaccion(CodEmpresa, solicitud);
        }

        public static ErrorDto<string> NumeroALetras(decimal numero)
        {
            return FrmTesTransaccionesDb.NumeroALetras(numero);
        }

        public ErrorDto<string> fxTesBancoDocsValor(int CodEmpresa, int banco, string tipo)
        {
            return _db.fxTesBancoDocsValor(CodEmpresa, banco, tipo);
        }

        public ErrorDto<List<TesCuentasBancarias>> TES_TransaccionesCuentasBancarias_Obtener(int CodEmpresa, string identificacion, string banco)
        {
            return _db.TES_TransaccionesCuentasBancarias_Obtener(CodEmpresa, identificacion, banco);
        }

        public ErrorDto<CtnxCuentasDto> ObtenerCuentas(int CodEmpresa, string jCuenta)
        {
            CuentaVarModel cuenta = JsonConvert.DeserializeObject<CuentaVarModel>(jCuenta);
            return _db.ObtenerCuentas(CodEmpresa, cuenta);
        }

        public ErrorDto<TesCuentasBancarias> TES_TransaccionesCtaInterna_Obtener(int CodEmpresa, int id_banco)
        {
            return _db.TES_TransaccionesCtaInterna_Obtener(CodEmpresa, id_banco);
        }
    }
}
