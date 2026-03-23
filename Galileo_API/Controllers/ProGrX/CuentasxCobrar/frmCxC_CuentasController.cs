using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.CuentasxCobrar;
using Galileo_API.Models.ProGrX.CuentasxCobrar;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.CuentasxCobrar
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCxcCuentasController : ControllerBase
    {
        private readonly FrmCxcCuentasBL _BL;
       

        public FrmCxcCuentasController(IConfiguration config)
        {
            _BL = new FrmCxcCuentasBL(config);
            
        }

        [HttpGet("fxFechaServidor")]
        public DateTime fxFechaServidor(int CodEmpresa)
        {
            return _BL.fxFechaServidor(CodEmpresa);
        }

        [HttpGet("fxCxC_Parametro")]
        public ErrorDto<string> fxCxC_Parametro(int codEmpresa, string codParametro)
        {
            return _BL.fxCxC_Parametro(codEmpresa, codParametro);
        }

        [HttpGet("CxCCuentasBusquedaOperacionLista_Obtener")]
        public ErrorDto<CxCCuentasBusquedaOperacionLista> CxCCuentasBusquedaOperacionLista_Obtener(
           int CodEmpresa,
           string filtros,
           bool esExportar = false)
        {
            return _BL.CxCCuentasBusquedaOperacionLista_Obtener(CodEmpresa, filtros, esExportar);
        }

        [HttpGet("CxCCuentasOperacion_Obtener")]
        public ErrorDto<CxCCuentasBusquedaOperacionItem> CxCCuentasOperacion_Obtener(int CodEmpresa, long operacion)
        {
            return _BL.CxCCuentasOperacion_Obtener(CodEmpresa, operacion);
        }

        [HttpGet("CxCCuentasOperacionScroll_Obtener")]
        public ErrorDto<CxCCuentasBusquedaOperacionItem> CxCCuentasOperacionScroll_Obtener(int CodEmpresa, long operacion, int tipo)
        {
            return _BL.CxCCuentasOperacionScroll_Obtener(CodEmpresa, operacion, tipo);
        }

        [HttpGet("CxCCuentas_Divisas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CxCCuentas_Divisas_Obtener(int CodEmpresa, int codContabilidad)
        {
            return _BL.CxCCuentas_Divisas_Obtener(CodEmpresa, codContabilidad);
        }

        [HttpGet("CxCCuentas_Oficinas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CxCCuentas_Oficinas_Obtener(int CodEmpresa)
        {
            return _BL.CxCCuentas_Oficinas_Obtener(CodEmpresa);
        }

        [HttpGet("CxCCuentas_Bancos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CxCCuentas_Bancos_Obtener(int CodEmpresa)
        {
            return _BL.CxCCuentas_Bancos_Obtener(CodEmpresa);
        }

        [HttpGet("CxCCuentas_Consulta_Obtener")]
        public ErrorDto<CxCCuentasConsultaData> CxCCuentas_Consulta_Obtener(int CodEmpresa, long operacion)
        {
            return _BL.CxCCuentas_Consulta_Obtener(CodEmpresa, operacion);
        }

        [HttpGet("CxCCuentasFacturas_Obtener")]
        public ErrorDto<CxCCuentasFacturasLista> CxCCuentasFacturas_Obtener(int CodEmpresa, long operacion)
        {
            return _BL.CxCCuentasFacturas_Obtener(CodEmpresa, operacion);
        }

        [HttpGet("CxCCuentasFacturasAdelantadas_Obtener")]
        public ErrorDto<CxCCuentasFacturasAdelantadasLista> CxCCuentasFacturasAdelantadas_Obtener(
            int CodEmpresa,
            string cedula,
            string cedulaPagador)
        {
            return _BL.CxCCuentasFacturasAdelantadas_Obtener(CodEmpresa, cedula, cedulaPagador);
        }

        [HttpGet("CxCCuentasPersonasFiltro_Obtener")]
        public ErrorDto<CxCCuentasPersonasFiltroLista> CxCCuentasPersonasFiltro_Obtener(
            int CodEmpresa,
            string filtros,
            bool esExportar = false)
        {
            return _BL.CxCCuentasPersonasFiltro_Obtener(CodEmpresa, filtros, esExportar);
        }

        [HttpGet("CxCCuentasPersonaFiltroPorCedula_Obtener")]
        public ErrorDto<CxCCuentasPersonasFiltroItem> CxCCuentasPersonaFiltroPorCedula_Obtener(int CodEmpresa, string cedula)
        {
            return _BL.CxCCuentasPersonaFiltroPorCedula_Obtener(CodEmpresa, cedula);
        }

        [HttpGet("CxCCuentasConcepto_Obtener")]
        public ErrorDto<CxCCuentasConceptoData> CxCCuentasConcepto_Obtener(int CodEmpresa, string codConcepto)
        {
            return _BL.CxCCuentasConcepto_Obtener(CodEmpresa, codConcepto);
        }

        [HttpGet("CxCCuentasConceptoScroll_Obtener")]
        public ErrorDto<CxCCuentasConceptosFiltroItem> CxCCuentasConceptoScroll_Obtener(int CodEmpresa, string codConcepto, int tipo)
        {
            return _BL.CxCCuentasConceptoScroll_Obtener(CodEmpresa, codConcepto, tipo);
        }

        [HttpGet("CxCCuentasConceptosFiltro_Obtener")]
        public ErrorDto<CxCCuentasBusquedaGenericaLista<CxCCuentasConceptosFiltroItem>> CxCCuentasConceptosFiltro_Obtener(
            int CodEmpresa,
            string filtros,
            bool esExportar = false)
        {
            return _BL.CxCCuentasConceptosFiltro_Obtener(CodEmpresa, filtros, esExportar);
        }

        [HttpGet("CxCCuentasContratoDetalle_Obtener")]
        public ErrorDto<CxCCuentasContratoData> CxCCuentasContratoDetalle_Obtener(int CodEmpresa, string codContrato, string cedula)
        {
            return _BL.CxCCuentasContratoDetalle_Obtener(CodEmpresa, codContrato, cedula);
        }

        [HttpGet("CxCCuentasContratoScroll_Obtener")]
        public ErrorDto<CxCCuentasContratosFiltroItem> CxCCuentasContratoScroll_Obtener(
            int CodEmpresa,
            string cedula,
            string codConcepto,
            string codContrato,
            int tipo)
        {
            return _BL.CxCCuentasContratoScroll_Obtener(CodEmpresa, cedula, codConcepto, codContrato, tipo);
        }

        [HttpGet("CxCCuentasContratosFiltro_Obtener")]
        public ErrorDto<CxCCuentasBusquedaGenericaLista<CxCCuentasContratosFiltroItem>> CxCCuentasContratosFiltro_Obtener(
            int CodEmpresa,
            string cedula,
            string codConcepto,
            string filtros,
            bool esExportar = false)
        {
            return _BL.CxCCuentasContratosFiltro_Obtener(CodEmpresa, cedula, codConcepto, filtros, esExportar);
        }

        [HttpGet("CxCCuentasPagador_Obtener")]
        public ErrorDto<CxCCuentasPagadorData> CxCCuentasPagador_Obtener(
            int CodEmpresa,
            string cedulaCliente,
            string codContrato,
            string cedulaPagador)
        {
            return _BL.CxCCuentasPagador_Obtener(CodEmpresa, cedulaCliente, codContrato, cedulaPagador);
        }

        [HttpGet("CxCCuentasPagadorScroll_Obtener")]
        public ErrorDto<CxCCuentasPagadoresFiltroItem> CxCCuentasPagadorScroll_Obtener(
            int CodEmpresa,
            string cedulaCliente,
            string codContrato,
            string cedulaPagador,
            int tipo)
        {
            return _BL.CxCCuentasPagadorScroll_Obtener(CodEmpresa, cedulaCliente, codContrato, cedulaPagador, tipo);
        }

        [HttpGet("CxCCuentasPagadoresFiltro_Obtener")]
        public ErrorDto<CxCCuentasBusquedaGenericaLista<CxCCuentasPagadoresFiltroItem>> CxCCuentasPagadoresFiltro_Obtener(
            int CodEmpresa,
            string cedulaCliente,
            string codContrato,
            string filtros,
            bool esExportar = false)
        {
            return _BL.CxCCuentasPagadoresFiltro_Obtener(CodEmpresa, cedulaCliente, codContrato, filtros, esExportar);
        }

        [HttpGet("CxCCuentasAutorizado_Obtener")]
        public ErrorDto<CxCCuentasAutorizadoData> CxCCuentasAutorizado_Obtener(
            int CodEmpresa,
            string cedulaCliente,
            string cedulaAutorizado)
        {
            return _BL.CxCCuentasAutorizado_Obtener(CodEmpresa, cedulaCliente, cedulaAutorizado);
        }

        [HttpGet("CxCCuentasAutorizadoScroll_Obtener")]
        public ErrorDto<CxCCuentasAutorizadosFiltroItem> CxCCuentasAutorizadoScroll_Obtener(
            int CodEmpresa,
            string cedulaCliente,
            string cedulaAutorizado,
            int tipo)
        {
            return _BL.CxCCuentasAutorizadoScroll_Obtener(CodEmpresa, cedulaCliente, cedulaAutorizado, tipo);
        }

        [HttpGet("CxCCuentasAutorizadosFiltro_Obtener")]
        public ErrorDto<CxCCuentasBusquedaGenericaLista<CxCCuentasAutorizadosFiltroItem>> CxCCuentasAutorizadosFiltro_Obtener(
            int CodEmpresa,
            string cedulaCliente,
            string filtros,
            bool esExportar = false)
        {
            return _BL.CxCCuentasAutorizadosFiltro_Obtener(CodEmpresa, cedulaCliente, filtros, esExportar);
        }

        [HttpGet("CxCCuentasCuentasBancarias_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CxCCuentasCuentasBancarias_Obtener(int CodEmpresa, string cedula, string banco)
        {
            return _BL.CxCCuentasCuentasBancarias_Obtener(CodEmpresa, cedula, banco);
        }

        [HttpPost("CxCCuentasFactura_Registra")]
        public ErrorDto<CxCCuentasFacturaMantenimientoResult> CxCCuentasFactura_Registra(
            int CodEmpresa,
            [FromBody] CxCCuentasFacturaRegistraRequest request)
        {
            return _BL.CxCCuentasFactura_Registra(CodEmpresa, request);
        }

        [HttpPost("CxCCuentasFactura_Elimina")]
        public ErrorDto<CxCCuentasFacturaMantenimientoResult> CxCCuentasFactura_Elimina(
            int CodEmpresa,
            [FromBody] CxCCuentasFacturaEliminaRequest request)
        {
            return _BL.CxCCuentasFactura_Elimina(CodEmpresa, request);
        }

        [HttpPost("CxCCuentasFactura_Vincular")]
        public ErrorDto<CxCCuentasFacturaMantenimientoResult> CxCCuentasFactura_Vincular(
            int CodEmpresa,
            [FromBody] CxCCuentasFacturaVincularRequest request)
        {
            return _BL.CxCCuentasFactura_Vincular(CodEmpresa, request);
        }

        [HttpPost("CxCCuentasFactura_CargarArchivo")]
        public ErrorDto<CxCCuentasFacturaMantenimientoResult> CxCCuentasFactura_CargarArchivo(
            int CodEmpresa,
            [FromBody] CxCCuentasFacturaCargaRequest request)
        {
            return _BL.CxCCuentasFactura_CargarArchivo(CodEmpresa, request);
        }

        [HttpPost("CxCCuentas_Guardar")]
        public ErrorDto<long> CxCCuentas_Guardar(int CodEmpresa, [FromBody] CxCCuentasSaveParams param)
        {
            return _BL.CxCCuentas_Guardar(CodEmpresa, param);
        }

        [HttpPost("CxCCuentasActivacion_Verifica")]
        public ErrorDto<CxCCuentasActivacionVerificaResult> CxCCuentasActivacion_Verifica(
    int CodEmpresa,
    [FromBody] CxCCuentasActivacionRequest request)
        {
            return _BL.CxCCuentasActivacion_Verifica(CodEmpresa, request);
        }

        [HttpPost("CxCCuentasActivacion_Activar")]
        public ErrorDto<bool> CxCCuentasActivacion_Activar(
            int CodEmpresa,
            [FromBody] CxCCuentasActivacionRequest request)
        {
            return _BL.CxCCuentasActivacion_Activar(CodEmpresa, request);
        }

        [HttpPost("CxCCuentasAnulacion_Verifica")]
        public ErrorDto<CxCCuentasAnulacionVerificaResult> CxCCuentasAnulacion_Verifica(
    int CodEmpresa,
    [FromBody] CxCCuentasAnulacionRequest request)
        {
            return _BL.CxCCuentasAnulacion_Verifica(CodEmpresa, request);
        }

        [HttpPost("CxCCuentasAnulacion_Anular")]
        public ErrorDto<bool> CxCCuentasAnulacion_Anular(
            int CodEmpresa,
            [FromBody] CxCCuentasAnulacionRequest request)
        {
            return _BL.CxCCuentasAnulacion_Anular(CodEmpresa, request);
        }

        [HttpPost("CxCCuentasActivacionDetalle_Obtener")]
        public ErrorDto<CxCCuentasActivacionDetalleResult> CxCCuentasActivacionDetalle_Obtener(
    int CodEmpresa,
    [FromBody] CxCCuentasActivacionDetalleRequest request)
        {
            return _BL.CxCCuentasActivacionDetalle_Obtener(CodEmpresa, request);
        }
    }
}
