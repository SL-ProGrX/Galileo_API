using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc; 
using Galileo.BusinessLogic.ProGrX_Nucleo;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmSifConsultaDocumentosController : ControllerBase
    {
        private readonly FrmSifConsultaDocumentosBL _bl;
        public FrmSifConsultaDocumentosController(IConfiguration config)
        {
            _bl = new FrmSifConsultaDocumentosBL(config);
        }

        [Authorize]
        [HttpGet("SifConsultaDocumentos_CajaUltimaApertura_Consultar")]
        public ErrorDto<int> SifConsultaDocumentos_CajaUltimaApertura_Consultar(int CodEmpresa, string pCajas)
        {
            return _bl.SifConsultaDocumentos_CajaUltimaApertura_Consultar(CodEmpresa, pCajas);
        }

        [Authorize]
        [HttpPost("SifConsultaDocumentos_Transaccion_Actualizar")]
        public ErrorDto SifConsultaDocumentos_Transaccion_Actualizar(int CodEmpresa, string usuario,string tipoDocumento, string codTransaccion, string actDocumento = "", string antDocumento = "")
        {
            return _bl.SifConsultaDocumentos_Transaccion_Actualizar(CodEmpresa, usuario, actDocumento, antDocumento, tipoDocumento, codTransaccion);
        }

        [Authorize]
        [HttpPost("SifConsultaDocumentos_ReciboDigitar_Enviar")]
        public ErrorDto SifConsultaDocumentos_ReciboDigitar_Enviar(int CodEmpresa, string codigo, string tipoDocumento, string formato)
        {
            return _bl.SifConsultaDocumentos_ReciboDigitar_Enviar(CodEmpresa, codigo, tipoDocumento, formato);
        }

        [Authorize]
        [HttpGet("SifConsultaDocumentos_FormasDePago_Obtener")]
        public ErrorDto<List<SifConsultaDocsFormasDePagoData>> SifConsultaDocumentos_FormasDePago_Obtener(int CodEmpresa, string tipoDocumento, string codTransaccion)
        {
            return _bl.SifConsultaDocumentos_FormasDePago_Obtener(CodEmpresa, tipoDocumento, codTransaccion);
        }

        [Authorize]
        [HttpGet("SifConsultaDocumentos_Seguimiento_Obtener")]
        public ErrorDto<SifConsultaDocSeguimientoData> SifConsultaDocumentos_Seguimiento_Obtener(int CodEmpresa, string tipoDocumento, string codTransaccion)
        {
            return _bl.SifConsultaDocumentos_Seguimiento_Obtener(CodEmpresa, tipoDocumento, codTransaccion);
        }

        [Authorize]
        [HttpGet("SifConsultaDocumentos_CargaDocumento_Obtener")]
        public ErrorDto<SifConsultaDocCargaDocumentoData> SifConsultaDocumentos_CargaDocumento_Obtener(int CodEmpresa, string tipoDocumento, string codTransaccion)
        {
            return _bl.SifConsultaDocumentos_CargaDocumento_Obtener(CodEmpresa, tipoDocumento, codTransaccion);
        }

        [Authorize]
        [HttpGet("SifConsultaDocumentos_CargaAsiento_Obtener")]
        public ErrorDto<List<SifConsultaDocCargaAsientoData>> SifConsultaDocumentos_CargaAsiento_Obtener(int CodEmpresa, string tipoDocumento, string codTransaccion)
        {
            return _bl.SifConsultaDocumentos_CargaAsiento_Obtener(CodEmpresa, tipoDocumento, codTransaccion);
        }

        [Authorize]
        [HttpGet("SifConsultaDocumentos_NombreDocumento_Consultar")]
        public ErrorDto<string> SifConsultaDocumentos_NombreDocumento_Consultar(int CodEmpresa, string tipoDocumento)
        {
            return _bl.SifConsultaDocumentos_NombreDocumento_Consultar(CodEmpresa, tipoDocumento);
        }

        [Authorize]
        [HttpPost("SifConsultaDocumentos_Reversar_Actualizar")]
        public ErrorDto SifConsultaDocumentos_Reversar_Actualizar(int CodEmpresa, string usuario, string documento, string tipoDocumento)
        {
            return _bl.SifConsultaDocumentos_Reversar_Actualizar(CodEmpresa, usuario, documento, tipoDocumento);
        }

        [Authorize]
        [HttpGet("SifConsultaDocumentos_Cajas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> SifConsultaDocumentos_Cajas_Obtener(int CodEmpresa)
        {
            return _bl.SifConsultaDocumentos_Cajas_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("SifConsultaDocumentos_FormasPago_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> SifConsultaDocumentos_FormasPago_Obtener(int CodEmpresa)
        {
            return _bl.SifConsultaDocumentos_FormasPago_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("SifConsultaDocumentos_Bancos")]
        public ErrorDto<List<DropDownListaGenericaModel>> SifConsultaDocumentos_Bancos(int CodEmpresa)
        {
            return _bl.SifConsultaDocumentos_Bancos(CodEmpresa);
        }

        [Authorize]
        [HttpGet("SifConsultaDocumentos_NombreUsuario_Consultar")]
        public ErrorDto<string> SifConsultaDocumentos_NombreUsuario_Consultar(int CodEmpresa, string usuario)
        {
            return _bl.SifConsultaDocumentos_NombreUsuario_Consultar(CodEmpresa, usuario);
        }

        [Authorize]
        [HttpGet("SifConsultaDocumentos_CuentasPorCobrar_Obtener")]
        public ErrorDto<List<SifConsultaDocCuentasPorCobrarData>> SifConsultaDocumentos_CuentasPorCobrar_Obtener(int CodEmpresa, string documento, string codigo)
        {
            return _bl.SifConsultaDocumentos_CuentasPorCobrar_Obtener(CodEmpresa, documento, codigo);
        }

        [Authorize]
        [HttpGet("SifConsultaDocumentos_Patrimonios_Obtener")]
        public ErrorDto<List<SifConsultaDocPatrimoniosData>> SifConsultaDocumentos_Patrimonios_Obtener(int CodEmpresa, string documento, string codigo)
        {
            return _bl.SifConsultaDocumentos_Patrimonios_Obtener(CodEmpresa, documento, codigo);
        }

        [Authorize]
        [HttpGet("SifConsultaDocumentos_Fondos_Obtener")]
        public ErrorDto<List<SifConsultaDocFondosData>> SifConsultaDocumentos_Fondos_Obtener(int CodEmpresa, string documento, string codigo)
        {
            return _bl.SifConsultaDocumentos_Fondos_Obtener(CodEmpresa, documento, codigo);
        }

        [Authorize]
        [HttpGet("SifConsultaDocumentos_Creditos_Obtener")]
        public ErrorDto<List<SifConsultaDocCreditosData>> SifConsultaDocumentos_Creditos_Obtener(int CodEmpresa, string documento, string codigo)
        {
            return _bl.SifConsultaDocumentos_Creditos_Obtener(CodEmpresa, documento, codigo);
        }

        [Authorize]
        [HttpGet("SifConsultaDocumentos_UltDocumento_Consultar")]
        public ErrorDto<string> SifConsultaDocumentos_UltDocumento_Consultar(int CodEmpresa, string tipoDocumento)
        {
            return _bl.SifConsultaDocumentos_UltDocumento_Consultar(CodEmpresa, tipoDocumento);
        }

        [Authorize]
        [HttpGet("SifConsultaDocumentos_SiguienteTransaccion_Consultar")]
        public ErrorDto<string> SifConsultaDocumentos_SiguienteTransaccion_Consultar(int CodEmpresa, int orden, string tipoDocumento = "", string transaccion = "")
        {
            return _bl.SifConsultaDocumentos_SiguienteTransaccion_Consultar(CodEmpresa, tipoDocumento, transaccion, orden);
        }

        [Authorize]
        [HttpGet("SifConsultaDocumentos_Documentos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> SifConsultaDocumentos_Documentos_Obtener(int CodEmpresa, string filtro= "")
        {
            return _bl.SifConsultaDocumentos_Documentos_Obtener(CodEmpresa, filtro);
        }

        [Authorize]
        [HttpGet("SifConsultaDocumentos_TipoConceptos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> SifConsultaDocumentos_TipoConceptos_Obtener(int CodEmpresa, string filtro= "")
        {
            return _bl.SifConsultaDocumentos_TipoConceptos_Obtener(CodEmpresa, filtro);
        }

        [Authorize]
        [HttpGet("SifConsultaDocumentos_UsuariosCajas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> SifConsultaDocumentos_UsuariosCajas_Obtener(int CodEmpresa, string caja)
        {
            return _bl.SifConsultaDocumentos_UsuariosCajas_Obtener(CodEmpresa, caja);
        }

        [Authorize]
        [HttpGet("SifConsultaDocumentos_Buscar")]
        public ErrorDto<SifConsultaDocTrasaccionesDataLista> SifConsultaDocumentos_Buscar(int CodEmpresa, bool esExportar, [FromQuery] SifConsultaDocFiltros filtros)
        {
            return _bl.SifConsultaDocumentos_Buscar(CodEmpresa, esExportar,filtros);
        }
        [Authorize]
        [HttpGet("SifConsultaDocumentos_Reporte")]
        public ErrorDto<object> SifConsultaDocumentos_Reporte(int CodEmpresa, string usuario, string tipoDocumento, string transaccion)
        {
            return _bl.SifConsultaDocumentos_Reporte(CodEmpresa, usuario, tipoDocumento, transaccion);
        }
    }
}