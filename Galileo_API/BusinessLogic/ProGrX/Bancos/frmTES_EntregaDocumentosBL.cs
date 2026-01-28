using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo_API.DataBaseTier;

namespace Galileo_API.BusinessLogic
{
    public class FrmTesEntregaDocumentosBL
    {
        private readonly FrmTesEntregaDocumentosDB EntregaDocumentosDb;

        public FrmTesEntregaDocumentosBL(IConfiguration config)
        {
            EntregaDocumentosDb = new FrmTesEntregaDocumentosDB(config);
        }

        public ErrorDto<List<DropDownListaBancosDocumentos>> Tes_Bancos_Obtener(int CodEmpresa)
        {
            return EntregaDocumentosDb.Tes_Bancos_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaTiposDocumentos>> Tes_Tipos_Obtener(int CodEmpresa, string cod_Banco)
        {
            return EntregaDocumentosDb.Tes_Tipos_Obtener(CodEmpresa, cod_Banco);
        }

        public ErrorDto<List<EntregaDocumentoPendientesDto>> listaPendientes_Obtener(int CodEmpresa, string filtros)
        {
            return EntregaDocumentosDb.listaPendientes_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto TES_documentosPendientes_Guardar(int CodEmpresa, string trasladoLista, string estadoCheck, string usuario)
        {
            return EntregaDocumentosDb.TES_documentosPendientes_Guardar(CodEmpresa, trasladoLista, estadoCheck, usuario);
        }


    }
}