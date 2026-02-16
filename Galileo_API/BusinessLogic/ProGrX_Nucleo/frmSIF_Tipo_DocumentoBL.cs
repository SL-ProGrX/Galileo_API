
using Galileo.DataBaseTier.ProGrX_Nucleo;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;
using Galileo.Models;

namespace Galileo.BusinessLogic.ProGrX_Nucleo
{
    public class FrmSifTipoDocumentoBL(IConfiguration config)
    {
        private readonly FrmSifTipoDocumentoDB _db = new FrmSifTipoDocumentoDB(config);

        public ErrorDto<string> SIF_tipoDocumento_Consultar(int CodEmpresa, string tipoDocumento, int orden)
        {
            return _db.SIF_tipoDocumento_Consultar(CodEmpresa, tipoDocumento, orden);
        }

        public ErrorDto<SifTipoDocumentoData> SIF_tipoDocumentoData_Consultar(int CodEmpresa, string tipoDocumento)
        {
            return _db.SIF_tipoDocumentoData_Consultar(CodEmpresa, tipoDocumento);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> SIF_tipoDocumento_Obtener(int CodEmpresa)
        {
            return _db.SIF_tipoDocumento_Obtener(CodEmpresa);
        }

        public ErrorDto SIF_tipoDocumento_Guardar(int CodEmpresa, string usuario, SifTipoDocumentoData tipoDoc,int accion)
        {
            return accion == 1
                ? _db.SIF_tipoDocumento_Insertar(CodEmpresa, usuario, tipoDoc)
                : _db.SIF_tipoDocumento_Actualiza(CodEmpresa, usuario, tipoDoc);
        }

        public ErrorDto<List<SifTipoDocConceptoData>> SIF_TipoDocumentosConceptosRelacionados_Obtener(int CodEmpresa, string tipoDoc)
        {
            return _db.SIF_TipoDocumentosConceptosRelacionados_Obtener(CodEmpresa, tipoDoc);
        }

        public ErrorDto SIF_TipoDocumentosConceptosRelacionados_Guardar(int CodEmpresa, string usuario, string cod_concepto, string tipoDoc, string accion)
        {
            return _db.SIF_TipoDocumentosConceptosRelacionados_Guardar(CodEmpresa, usuario, cod_concepto, tipoDoc, accion);
        }
    }
}