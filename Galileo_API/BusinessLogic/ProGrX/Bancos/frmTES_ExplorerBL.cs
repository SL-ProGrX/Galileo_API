using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo_API.DataBaseTier;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic
{
    public class FrmTesExplorerBL
    {
        private readonly FrmTesExplorerDB ExplorerDb;

        public FrmTesExplorerBL(IConfiguration config)
        {
            ExplorerDb = new FrmTesExplorerDB(config);
        }

        public ErrorDto<List<TesDropDownListaBancosExplorer>> Tes_Bancos_Obtener(int CodEmpresa)
        {
            return ExplorerDb.Tes_Bancos_Obtener(CodEmpresa);
        }

        public ErrorDto<TablasListaGenericaModel> TES_explorer_Obtener(int CodEmpresa, string filtrosExplorer, string filtros)
        {
            FiltrosLazyLoadData filtro = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(filtros) ?? new FiltrosLazyLoadData();
            return ExplorerDb.TES_explorer_Obtener(CodEmpresa, filtrosExplorer, filtro);
        }



    }
}