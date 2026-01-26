using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo_API.DataBaseTier;

namespace Galileo_API.BusinessLogic
{
    public class FrmTesDocumentesDupBL
    {
        private readonly FrmTesDocumentesDupDB DocumentoDuplicadoDb;

        public FrmTesDocumentesDupBL(IConfiguration config)
        {
            DocumentoDuplicadoDb = new FrmTesDocumentesDupDB(config);
        }

        public ErrorDto<List<DropDownListaBancos>> Tes_Bancos_Obtener(int CodEmpresa)
        {
            return DocumentoDuplicadoDb.Tes_Bancos_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaTipos>> Tes_Tipos_Obtener(int CodEmpresa, string cod_Banco)
        {
            return DocumentoDuplicadoDb.Tes_Tipos_Obtener(CodEmpresa, cod_Banco);
        }

        public ErrorDto<List<DocumentoDuplicadosLista>> Documentos_Duplicados_Obtener(int CodEmpresa, string filtros)
        {
            return DocumentoDuplicadoDb.Documentos_Duplicados_Obtener(CodEmpresa, filtros);
        }


    }
}