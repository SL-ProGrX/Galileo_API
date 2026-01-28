

using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using PgxAPI.DataBaseTier.ProGrX.Bancos;

namespace PgxAPI.BusinessLogic.ProGrX.Bancos
{
    public class FrmTesDocumentosBL
    {

        private readonly FrmTesDocumentosDB _documentosDb;
        public FrmTesDocumentosBL(IConfiguration config)
        {
            _documentosDb = new FrmTesDocumentosDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> TES_DocumentosLista_Obtener(int CodEmpresa)
        {
            return _documentosDb.TES_DocumentosLista_Obtener(CodEmpresa);
        }

        public ErrorDto<TesTiposDocDto> Tes_Documentos_Scroll(int CodEmpresa, string tipo, int? scroll)
        {
            return _documentosDb.Tes_Documentos_Scroll(CodEmpresa, tipo, scroll);
        }

        public ErrorDto<TesTiposDocDto> Tes_Documentos_Obtener(int CodEmpresa, string tipo)
        {
            return _documentosDb.Tes_Documentos_Obtener(CodEmpresa, tipo);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> TES_DocumentosTiposAsientos_Obtener(int CodEmpresa,int contabilidad)
        {
            return _documentosDb.TES_DocumentosTiposAsientos_Obtener(CodEmpresa, contabilidad);
        }

        public ErrorDto<List<TesDocAnulaConceptosData>> TES_DocAnulaConceptos_Obtener(int CodEmpresa, string tipo)
        {
            return _documentosDb.TES_DocAnulaConceptos_Obtener(CodEmpresa, tipo);
        }

        public ErrorDto<DropDownListaGenericaModel> Tes_DocAnulaConceptos_Scroll(int CodEmpresa, string concepto, int? scroll)
        {
            return _documentosDb.Tes_DocAnulaConceptos_Scroll(CodEmpresa, concepto, scroll);
        }

        public ErrorDto TES_Documentos_Guardar(int CodEmpresa, string usuario, TesTiposDocDto documento)
        {
            return _documentosDb.TES_Documentos_Guardar(CodEmpresa, usuario, documento);
        }

        public ErrorDto TES_Documentos_Eliminar(int CodEmpresa, string tipo, string usuario)
        {
            return _documentosDb.TES_Documentos_Eliminar(CodEmpresa, tipo, usuario);
        }

        public ErrorDto TES_DocAnulaConcepto_Guardar(int CodEmpresa, string usuario, string tipo, TesDocAnulaConceptosData concepto)
        {
            return _documentosDb.TES_DocAnulaConcepto_Guardar(CodEmpresa, usuario, tipo, concepto);
        }

        public ErrorDto TES_DocAnulaConcepto_Eliminar(int CodEmpresa, int id_conceptos, string usuario)
        {
            return _documentosDb.TES_DocAnulaConcepto_Eliminar(CodEmpresa, id_conceptos, usuario);
        }

    }
}
