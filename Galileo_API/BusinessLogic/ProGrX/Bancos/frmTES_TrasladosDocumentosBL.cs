using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo_API.DataBaseTier.ProGrX.Bancos;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic.ProGrX.Bancos
{
    public class FrmTesTrasladosDocumentosBL
    {
        private readonly  FrmTesTrasladosDocumentosDB TrasladosDocumentosDb;

        public FrmTesTrasladosDocumentosBL(IConfiguration config)
        {
            TrasladosDocumentosDb = new FrmTesTrasladosDocumentosDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> TES_TrasladosDoc_Ubicaciones_Obtener(int CodEmpresa, string Usuario, string Tipo)
        {
            return TrasladosDocumentosDb.TES_TrasladosDoc_Ubicaciones_Obtener(CodEmpresa, Usuario, Tipo);
        }

        public ErrorDto<TesUbiRemesaDto> TES_TrasladosDoc_Remesa_Scroll_Obtener(int CodEmpresa, int scrollCode, int Remesa)
        {
            return TrasladosDocumentosDb.TES_TrasladosDoc_Remesa_Scroll_Obtener(CodEmpresa, scrollCode, Remesa);
        }

        public ErrorDto<TesUbiRemesaDto> TES_TrasladosDoc_Remesa_Obtener(int CodEmpresa, int Remesa)
        {
            return TrasladosDocumentosDb.TES_TrasladosDoc_Remesa_Obtener(CodEmpresa, Remesa);
        }

        public ErrorDto<TablasListaGenericaModel> TES_TrasladosDocumentos_Obtener(int CodEmpresa, int Remesa, string filtros)
        {
            FiltrosLazyLoadData filtro = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(filtros) ?? new FiltrosLazyLoadData();
            return TrasladosDocumentosDb.TES_TrasladosDocumentos_Obtener(CodEmpresa, Remesa, filtro);
        }

        public ErrorDto<TesTrasladoDocumentoDto> TES_TrasladosDoc_Solicitud_Obtener(int CodEmpresa, int Solicitud)
        {
            return TrasladosDocumentosDb.TES_TrasladosDoc_Solicitud_Obtener(CodEmpresa, Solicitud);
        }

        public ErrorDto TES_TrasladosDocumentos_Guardar(int CodEmpresa, bool vEdita, string Remesa)
        {
            TesUbiRemesaDto remesa = JsonConvert.DeserializeObject<TesUbiRemesaDto>(Remesa) ?? new TesUbiRemesaDto();
            return TrasladosDocumentosDb.TES_TrasladosDocumentos_Guardar(CodEmpresa, vEdita, remesa);
        }

        public ErrorDto TES_TrasladosDocumentos_Eliminar(int CodEmpresa, int Remesa, string Usuario)
        {
            return TrasladosDocumentosDb.TES_TrasladosDocumentos_Eliminar(CodEmpresa, Remesa, Usuario);
        }

        public ErrorDto TES_TrasladosDocumentos_Linea_Guardar(int CodEmpresa, string Remesa, string Linea)
        {
            TesUbiRemesaDto remesa = JsonConvert.DeserializeObject<TesUbiRemesaDto>(Remesa) ?? new TesUbiRemesaDto();
            TesTrasladoDocumentoDto linea = JsonConvert.DeserializeObject<TesTrasladoDocumentoDto>(Linea) ?? new TesTrasladoDocumentoDto();
            return TrasladosDocumentosDb.TES_TrasladosDocumentos_Linea_Guardar(CodEmpresa, remesa, linea);
        }

        public ErrorDto TES_TrasladosDocumentos_Linea_Eliminar(int CodEmpresa, int Remesa, int Solicitud)
        {
            return TrasladosDocumentosDb.TES_TrasladosDocumentos_Linea_Eliminar(CodEmpresa, Remesa, Solicitud);
        }
    }
}
