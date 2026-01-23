
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo_API.DataBaseTier;

namespace Galileo_API.BusinessLogic.ProGrX.Bancos
{
    public class FrmTesChequesBancoPuenteBL
    {
        private readonly FrmTesChequesBancoPuenteDB _chequesBancoPuenteDB;

        public FrmTesChequesBancoPuenteBL(IConfiguration config)
        {
            _chequesBancoPuenteDB = new FrmTesChequesBancoPuenteDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> TES_BancosGestion_Obtener(int CodEmpresa, string usuario, string gestion)
        {
            return _chequesBancoPuenteDB.TES_BancosGestion_Obtener(CodEmpresa, usuario, gestion);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> TES_Bancos_Obtener(int CodEmpresa)
        {
            return _chequesBancoPuenteDB.TES_Bancos_Obtener(CodEmpresa);
        }

        public ErrorDto<List<ChequesBancoPuenteData>> TES_ChequePuenteLista_Obtener(int CodEmpresa, int id_banco)
        {
            return _chequesBancoPuenteDB.TES_ChequePuenteLista_Obtener(CodEmpresa, id_banco);
        }

        public ErrorDto TES_ChequesBanco_Aplica(int CodEmpresa, int id_banco, int banco, string usuario, List<ChequesBancoPuenteData> data)
        {
            return _chequesBancoPuenteDB.TES_ChequesBanco_Aplica(CodEmpresa, id_banco, banco, usuario, data);
        }
    }
}
