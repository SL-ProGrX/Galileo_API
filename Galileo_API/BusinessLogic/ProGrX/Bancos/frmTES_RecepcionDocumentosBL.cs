using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo_API.DataBaseTier.ProGrX.Bancos;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic.ProGrX.Bancos
{
    public class FrmTesRecepcionDocumentosBL
    {
        private readonly FrmTesRecepcionDocumentosDB RecepcionDocumentosDb;

        public FrmTesRecepcionDocumentosBL(IConfiguration config)
        {
            RecepcionDocumentosDb = new FrmTesRecepcionDocumentosDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> TES_RecepcionDoc_Ubicaciones_Obtener(int CodEmpresa, string Usuario)
        {
            return RecepcionDocumentosDb.TES_RecepcionDoc_Ubicaciones_Obtener(CodEmpresa, Usuario);
        }

        public ErrorDto<TesUbiRemesaDto> TES_RecepcionDoc_Remesa_Scroll_Obtener(int CodEmpresa, int scrollCode, int Remesa)
        {
            return RecepcionDocumentosDb.TES_RecepcionDoc_Remesa_Scroll_Obtener(CodEmpresa, scrollCode, Remesa);
        }

        public ErrorDto<TesUbiRemesaDto> TES_RecepcionDoc_Remesa_Obtener(int CodEmpresa, int Remesa)
        {
            return RecepcionDocumentosDb.TES_RecepcionDoc_Remesa_Obtener(CodEmpresa, Remesa);
        }

        public ErrorDto<TablasListaGenericaModel> TES_RecepcionDocumentos_Obtener(int CodEmpresa, int Remesa, string filtros)
        {
            FiltrosLazyLoadData filtro = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(filtros) ?? new FiltrosLazyLoadData();
            return RecepcionDocumentosDb.TES_RecepcionDocumentos_Obtener(CodEmpresa, Remesa, filtro);
        }

        public ErrorDto TES_RecepcionDocumentos_Aplicar(int CodEmpresa, string parametros)
        {
            TesRecepcionDocumentoFiltros filtros = JsonConvert.DeserializeObject<TesRecepcionDocumentoFiltros>(parametros) ?? new TesRecepcionDocumentoFiltros();
            return RecepcionDocumentosDb.TES_RecepcionDocumentos_Aplicar(CodEmpresa, filtros);
        }
    }
}
