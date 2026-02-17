 
using Galileo.DataBaseTier.ProGrX_Nucleo;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo; 

namespace Galileo.BusinessLogic.ProGrX_Nucleo
{
    public class FrmSifConsultaDocumentosBL(IConfiguration config)
    {
        private readonly FrmSifConsultaDocumentosDB _db = new(config);

        public ErrorDto<int> SifConsultaDocumentos_CajaUltimaApertura_Consultar(int CodEmpresa, string pCajas)
        {
            return _db.SifConsultaDocumentos_CajaUltimaApertura_Consultar(CodEmpresa, pCajas);
        }

        public ErrorDto SifConsultaDocumentos_Transaccion_Actualizar(int CodEmpresa, string usuario, string actDocumento, string antDocumento, string tipoDocumento, string codTransaccion)
        {
            return _db.SifConsultaDocumentos_Transaccion_Actualizar(CodEmpresa, usuario,  actDocumento,  antDocumento,  tipoDocumento,  codTransaccion);
        }

        public ErrorDto SifConsultaDocumentos_ReciboDigitar_Enviar(int CodEmpresa, string codigo, string tipoDocumento, string formato)
        {
            return _db.SifConsultaDocumentos_ReciboDigitar_Enviar(CodEmpresa, codigo, tipoDocumento, formato);
        }

        public ErrorDto<List<SifConsultaDocsFormasDePagoData>> SifConsultaDocumentos_FormasDePago_Obtener(int CodEmpresa, string tipoDocumento, string codTransaccion)
        {
            return _db.SifConsultaDocumentos_FormasDePago_Obtener(CodEmpresa, tipoDocumento, codTransaccion);
        }

        public ErrorDto<SifConsultaDocSeguimientoData> SifConsultaDocumentos_Seguimiento_Obtener(int CodEmpresa, string tipoDocumento, string codTransaccion)
        {
            return _db.SifConsultaDocumentos_Seguimiento_Obtener(CodEmpresa, tipoDocumento, codTransaccion);
        }

        public ErrorDto<SifConsultaDocCargaDocumentoData> SifConsultaDocumentos_CargaDocumento_Obtener(int CodEmpresa, string tipoDocumento, string codTransaccion)
        {
            return _db.SifConsultaDocumentos_CargaDocumento_Obtener(CodEmpresa, tipoDocumento, codTransaccion);
        }

        public ErrorDto<List<SifConsultaDocCargaAsientoData>> SifConsultaDocumentos_CargaAsiento_Obtener(int CodEmpresa, string tipoDocumento, string codTransaccion)
        {
            return _db.SifConsultaDocumentos_CargaAsiento_Obtener(CodEmpresa, tipoDocumento, codTransaccion);
        }

        public ErrorDto<string> SifConsultaDocumentos_NombreDocumento_Consultar(int CodEmpresa, string tipoDocumento)
        {
            return _db.SifConsultaDocumentos_NombreDocumento_Consultar(CodEmpresa, tipoDocumento);
        }

        public ErrorDto SifConsultaDocumentos_Reversar_Actualizar(int CodEmpresa, string usuario, string documento, string tipoDocumento)
        {
            return _db.SifConsultaDocumentos_Reversar_Actualizar(CodEmpresa, usuario, documento, tipoDocumento);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> SifConsultaDocumentos_Cajas_Obtener(int CodEmpresa)
        {
            return _db.SifConsultaDocumentos_Cajas_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> SifConsultaDocumentos_FormasPago_Obtener(int CodEmpresa)
        {
            return _db.SifConsultaDocumentos_FormasPago_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> SifConsultaDocumentos_Bancos(int CodEmpresa)
        {
            return _db.SifConsultaDocumentos_Bancos(CodEmpresa);
        }

        public ErrorDto<string> SifConsultaDocumentos_NombreUsuario_Consultar(int CodEmpresa, string usuario)
        {
            return _db.SifConsultaDocumentos_NombreUsuario_Consultar(CodEmpresa, usuario);
        }

        public ErrorDto<List<SifConsultaDocCuentasPorCobrarData>> SifConsultaDocumentos_CuentasPorCobrar_Obtener(int CodEmpresa, string documento, string codigo)
        {
            return _db.SifConsultaDocumentos_CuentasPorCobrar_Obtener(CodEmpresa, documento, codigo);
        }

        public ErrorDto<List<SifConsultaDocPatrimoniosData>> SifConsultaDocumentos_Patrimonios_Obtener(int CodEmpresa, string documento, string codigo)
        {
            return _db.SifConsultaDocumentos_Patrimonios_Obtener(CodEmpresa, documento, codigo);
        }
        public ErrorDto<List<SifConsultaDocFondosData>> SifConsultaDocumentos_Fondos_Obtener(int CodEmpresa, string documento, string codigo)
        {
            return _db.SifConsultaDocumentos_Fondos_Obtener(CodEmpresa, documento, codigo);
        }

        public ErrorDto<List<SifConsultaDocCreditosData>> SifConsultaDocumentos_Creditos_Obtener(int CodEmpresa, string documento, string codigo)
        {
            return _db.SifConsultaDocumentos_Creditos_Obtener(CodEmpresa, documento, codigo);
        }

        public ErrorDto<string> SifConsultaDocumentos_UltDocumento_Consultar(int CodEmpresa, string tipoDocumento)
        {
            return _db.SifConsultaDocumentos_UltDocumento_Consultar(CodEmpresa, tipoDocumento);
        }
        public ErrorDto<string> SifConsultaDocumentos_SiguienteTransaccion_Consultar(int CodEmpresa, string tipoDocumento, string transaccion, int orden)
        {
            return _db.SifConsultaDocumentos_SiguienteTransaccion_Consultar(CodEmpresa, tipoDocumento, transaccion, orden);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> SifConsultaDocumentos_Documentos_Obtener(int CodEmpresa, string filtro)
        {
            return _db.SifConsultaDocumentos_Documentos_Obtener(CodEmpresa, filtro);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> SifConsultaDocumentos_TipoConceptos_Obtener(int CodEmpresa, string filtro)
        {
            return _db.SifConsultaDocumentos_TipoConceptos_Obtener(CodEmpresa, filtro);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> SifConsultaDocumentos_UsuariosCajas_Obtener(int CodEmpresa, string caja)
        {
            return _db.SifConsultaDocumentos_UsuariosCajas_Obtener(CodEmpresa, caja);
        }

        public ErrorDto<SifConsultaDocTrasaccionesDataLista> SifConsultaDocumentos_Buscar(int CodEmpresa, bool esExportar, SifConsultaDocFiltros filtros)
        { 
            return _db.SifConsultaDocumentos_Buscar(CodEmpresa, esExportar, filtros);
        }
        
        public ErrorDto<object> SifConsultaDocumentos_Reporte(int CodEmpresa, string usuario, string tipoDocumento, string transaccion)
        {
            return _db.SifConsultaDocumentos_Reporte(CodEmpresa, usuario, tipoDocumento, transaccion);
        }
        
    }
}
