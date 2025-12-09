using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.TES;
using Galileo_API.BusinessLogic.ProGrX.Bancos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Bancos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmTesTransaccionesController : ControllerBase
    {
        private readonly FrmTesTransaccionesBL _bl;

        public FrmTesTransaccionesController(IConfiguration config)
        {
            _bl = new FrmTesTransaccionesBL(config);
        }

        [Authorize]
        [HttpGet("TES_Solicitudes_Obtener")]
        public ErrorDto<TablasListaGenericaModel> TES_Solicitudes_Obtener(int CodEmpresa, int contabilidad, string filtro)
        {
            return _bl.TES_Solicitudes_Obtener(CodEmpresa, contabilidad, filtro);
        }

        [Authorize]
        [HttpGet("TES_TiposDocumentos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> TES_TiposDocumentos_Obtener(int CodEmpresa, string Usuario, int id_banco, string? tipo = "S")
        {
            return _bl.TES_TiposDocumentos_Obtener(CodEmpresa, Usuario, id_banco, tipo);
        }

        [Authorize]
        [HttpGet("TES_Unidades_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> TES_Unidades_Obtener(int CodEmpresa, string usuario, int banco, int contabilidad)
        {
            return _bl.TES_Unidades_Obtener(CodEmpresa, usuario, banco, contabilidad);
        }

        [Authorize]
        [HttpGet("TiposIdentificacion_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> TiposIdentificacion_Obtener(int CodEmpresa)
        {
            return _bl.TiposIdentificacion_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("TES_Conceptos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> TES_Conceptos_Obtener(int CodEmpresa, string usuario, int banco)
        {
            return _bl.TES_Conceptos_Obtener(CodEmpresa, usuario, banco);
        }

        [Authorize]
        [HttpGet("TES_BancosCarga_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> TES_BancosCarga_Obtener(int CodEmpresa, string usuario, string gestion)
        {
            return _bl.TES_BancosCarga_Obtener(CodEmpresa, usuario, gestion);
        }

        [Authorize]
        [HttpGet("TES_Transaccion_Scroll")]
        public ErrorDto<int> TES_Transaccion_Scroll(int CodEmpresa, int scrollCode, string codigo, int contabilidad)
        {
            return _bl.TES_Transaccion_Scroll(CodEmpresa, scrollCode, codigo, contabilidad);
        }

        [Authorize]
        [HttpGet("TES_Transaccion_Obtener")]
        public ErrorDto<TesTransaccionDto> TES_Transaccion_Obtener(int CodEmpresa, int tesoreria, int contabilidad)
        {
            return _bl.TES_Transaccion_Obtener(CodEmpresa, tesoreria, contabilidad);
        }

        [Authorize]
        [HttpGet("TES_Afectaciones_Obtener")]
        public ErrorDto<List<TesAfectacionDto>> TES_Afectaciones_Obtener(int CodEmpresa, int tesoreria)
        {
            return _bl.TES_Afectaciones_Obtener(CodEmpresa, tesoreria);
        }

        [Authorize]
        [HttpGet("TES_TransaccionAsiento_Obtener")]
        public ErrorDto<List<TesTransAsientoDto>> TES_TransaccionAsiento_Obtener(string vSolicitud)
        {
            return _bl.TES_TransaccionAsiento_Obtener(vSolicitud);
        }

        [Authorize]
        [HttpGet("TES_Bitacora_Obtener")]
        public ErrorDto<List<TesBitacoraDto>> TES_Bitacora_Obtener(int CodEmpresa, int tesoreria)
        {
            return _bl.TES_Bitacora_Obtener(CodEmpresa, tesoreria);
        }

        [Authorize]
        [HttpGet("TES_Localizacion_Obtener")]
        public ErrorDto<List<TesLocalizacionDto>> TES_Localizacion_Obtener(int CodEmpresa, int solicitud)
        {
            return _bl.TES_Localizacion_Obtener(CodEmpresa, solicitud);
        }

        [Authorize]
        [HttpGet("TES_ReImpresiones_Obtener")]
        public ErrorDto<List<TesReimpresionesDto>> TES_ReImpresiones_Obtener(int CodEmpresa, int solicitud)
        {
            return _bl.TES_ReImpresiones_Obtener(CodEmpresa, solicitud);
        }

        [Authorize]
        [HttpGet("TES_CambioFechas_Obtener")]
        public ErrorDto<List<TesCambioFechasDto>> TES_CambioFechas_Obtener(int CodEmpresa, int solicitud)
        {
            return _bl.TES_CambioFechas_Obtener(CodEmpresa, solicitud);
        }

        [Authorize]
        [HttpGet("TES_TransaccionDocumento_Scroll")]
        public ErrorDto<int> TES_TransaccionDocumento_Scroll(int CodEmpresa, int scrollCode, string parametros)
        {
            return _bl.TES_TransaccionDocumento_Scroll(CodEmpresa, scrollCode, parametros);
        }

        [Authorize]
        [HttpGet("TES_TransaccionDoc_Obtener")]
        public ErrorDto<int> TES_TransaccionDoc_Obtener(
          int CodEmpresa,
          string documento,
          int banco,
          string tipo,
          int contabilidad)
        {
            return _bl.TES_TransaccionDoc_Obtener(
               CodEmpresa,
               documento,
               banco,
               tipo,
               contabilidad);
        }

        [Authorize]
        [HttpPatch("TES_CambioCuentaBancaria_Aplicar")]
        public ErrorDto TES_CambioCuentaBancaria_Aplicar(int CodEmpresa, string usuario, int solicitud, string cuenta)
        {
            return _bl.TES_CambioCuentaBancaria_Aplicar(CodEmpresa, usuario, solicitud, cuenta);
        }

        [Authorize]
        [HttpPost("TES_Transaccion_Guardar")]
        public ErrorDto TES_Transaccion_Guardar(int CodEmpresa, string usuario, int contabilidad, TesTransaccionDto transaccion)
        {
            return _bl.TES_Transaccion_Guardar(CodEmpresa, usuario, contabilidad, transaccion);
        }

        [Authorize]
        [HttpGet("TES_transaccionesBeneficiario_Obtener")]
        public ErrorDto<TablasListaGenericaModel> TES_transaccionesBeneficiario_Obtener(
            int CodEmpresa, string tipo, string filtro)
        {
            return _bl.TES_transaccionesBeneficiario_Obtener(CodEmpresa, tipo, filtro);
        }

        [Authorize]
        [HttpGet("TES_TransaccionesDivisa_Obtener")]
        public ErrorDto<TesDivisaAsiento> TES_TransaccionesDivisa_Obtener(int CodEmpresa, string cuenta, int contabilidad)
        {
            return _bl.TES_TransaccionesDivisa_Obtener(CodEmpresa, cuenta, contabilidad);
        }

        [Authorize]
        [HttpGet("TES_TransaccionesTC_Obtener")]
        public ErrorDto<float> TES_TransaccionesTC_Obtener(int CodEmpresa, string cod_divisa, int contabilidad)
        {
            return _bl.TES_TransaccionesTC_Obtener(CodEmpresa, cod_divisa, contabilidad);
        }

        [Authorize]
        [HttpGet("TES_TransaccionesSinpe_Obtener")]
        public ErrorDto TES_TransferenciasSinpe_Valida(int CodEmpresa, int solicitud, string usuario)
        {
            return _bl.TES_TransferenciasSinpe_Valida(CodEmpresa, solicitud, usuario);
        }

        [Authorize]
        [HttpGet("TES_TransaccionesUnidades_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> TES_TransaccionesUnidades_Obtener(int CodEmpresa, int contabilidad)
        {
            return _bl.TES_TransaccionesUnidades_Obtener(CodEmpresa, contabilidad);
        }

        [Authorize]
        [HttpGet("TES_TransaccionesCC_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> TES_TransaccionesCC_Obtener(int CodEmpresa, string unidad)
        {
            return _bl.TES_TransaccionesCC_Obtener(CodEmpresa, unidad);
        }

        [Authorize]
        [HttpGet("TES_ControlDivisas_Obtener")]
        public ErrorDto<TesControlDivisas> TesControlDivisas_Obtener(int CodEmpresa, int id_banco, int contabilidad)
        {
            return _bl.TesControlDivisas_Obtener(CodEmpresa, id_banco, contabilidad);
        }

        [Authorize]
        [HttpGet("TesEmpresaSinpe_Valida")]
        public ErrorDto<bool> TesEmpresaSinpe_Valida(int CodEmpresa)
        {
            return _bl.TesEmpresaSinpe_Valida(CodEmpresa);
        }

        [Authorize]
        [HttpDelete("TES_Transacciones_Eliminar")]
        public ErrorDto TES_Transacciones_Eliminar(int CodEmpresa, int solicitud, string usuario)
        {
            return _bl.TES_Transacciones_Eliminar(CodEmpresa, solicitud, usuario);
        }

        [Authorize]
        [HttpGet("Tes_BitacoraTransaccion")]
        public ErrorDto<List<TesBitacoraTransaccion>> Tes_BitacoraTransaccion(int CodEmpresa, string solicitud)
        {
            return _bl.Tes_BitacoraTransaccion(CodEmpresa, solicitud);
        }

        [Authorize]
        [HttpGet("Tes_NumeroALetras_Convertir")]
        public ErrorDto<string> NumeroALetras(decimal numero)
        {
            return _bl.NumeroALetras(numero);
        }

        [Authorize]
        [HttpGet("Tes_BancoValidaEmision")]
        public ErrorDto<string> fxTesBancoDocsValor(int CodEmpresa, int banco, string tipo)
        {
            return _bl.fxTesBancoDocsValor(CodEmpresa, banco, tipo);
        }

        [Authorize]
        [HttpGet("TES_TransaccionesCuentasBancarias_Obtener")]
        public ErrorDto<List<TesCuentasBancarias>> TES_TransaccionesCuentasBancarias_Obtener(int CodEmpresa, string identificacion, string banco)
        {
            return _bl.TES_TransaccionesCuentasBancarias_Obtener(CodEmpresa, identificacion, banco);
        }

        [Authorize]
        [HttpGet("ObtenerCuentas")]
        public ErrorDto<CtnxCuentasDto> ObtenerCuentas(int CodEmpresa, string cuenta)
        {
            return _bl.ObtenerCuentas(CodEmpresa, cuenta);
        }

        [Authorize]
        [HttpGet("TES_TransaccionesCtaInterna_Obtener")]
        public ErrorDto<TesCuentasBancarias> TES_TransaccionesCtaInterna_Obtener(int CodEmpresa, int id_banco)
        {
            return _bl.TES_TransaccionesCtaInterna_Obtener(CodEmpresa, id_banco);
        }
    }
}
