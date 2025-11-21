using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.GA;

namespace Galileo.BusinessLogic
{
    public class FrmGaDocumentosBl
    {
        readonly FrmGaDocumentosDb DbfrmGA_Documentos;

        public FrmGaDocumentosBl(IConfiguration config)
        {
            DbfrmGA_Documentos = new FrmGaDocumentosDb(config);
        }

        public ErrorDto<List<TiposDocumentosArchivosDto>> TiposDocumentos_Obtener(int CodEmpresa, string Usuario, string Modulo)
        {
            return DbfrmGA_Documentos.TiposDocumentos_Obtener(CodEmpresa, Usuario, Modulo);
        }


        public ErrorDto Documentos_Insertar(int CodEmpresa, DocumentosArchivoDto documentInfo)
        {
            return DbfrmGA_Documentos.Documentos_Insertar(CodEmpresa, documentInfo);
        }

        public List<DocumentosArchivoDto> Documentos_Obtener(GaDocumento filtros)
        {
            return DbfrmGA_Documentos.Documentos_Obtener(filtros);
        }

        public ErrorDto Documentos_Eliminar(int CodCliente, string llave01, string llave02, string llave03, string usuario)
        {
            return DbfrmGA_Documentos.Documentos_Eliminar(CodCliente, llave01, llave02, llave03, usuario);
        }
    }
}