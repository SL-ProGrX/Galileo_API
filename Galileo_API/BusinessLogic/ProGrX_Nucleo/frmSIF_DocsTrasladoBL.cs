using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX_Nucleo;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;

namespace Galileo.BusinessLogic.ProGrX_Nucleo
{
    public class FrmSifDocsTrasladoBL(IConfiguration config)
    {
        private readonly FrmSifDocsTrasladoDB _db = new FrmSifDocsTrasladoDB(config);

        public ErrorDto<SifDocsTrasladoDocumentosLista> Sif_DocsTraslado_Lista_Obtener(int CodEmpresa,string jfiltros,DateTime fechaInicio,DateTime fechaFin,bool soloBalanceados)
        {
            FiltrosLazyLoadData filtros = string.IsNullOrWhiteSpace(jfiltros) ? new FiltrosLazyLoadData() 
             : JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros)!;
            return _db.Sif_DocsTraslado_Lista_Obtener(CodEmpresa, filtros, fechaInicio, fechaFin, soloBalanceados);
        }

        public ErrorDto<List<SifDocsTrasladoDocumentosData>> Sif_DocsTraslado_Lista_Export(int CodEmpresa,string jfiltros,DateTime fechaInicio,DateTime fechaFin,bool soloBalanceados)
        {
            var filtros = string.IsNullOrWhiteSpace(jfiltros)
                ? new FiltrosLazyLoadData()
                : JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros);

            return _db.Sif_DocsTraslado_Lista_Export(CodEmpresa, filtros!, fechaInicio, fechaFin, soloBalanceados);
        }

        public ErrorDto<SifDocsTrasladoDesbalanceadosLista> Sif_DocsTraslado_Desbalanceados_Obtener(int CodEmpresa,string jfiltros,DateTime fechaInicio,DateTime fechaFin)
        {
            FiltrosLazyLoadData filtros = string.IsNullOrWhiteSpace(jfiltros) ? new FiltrosLazyLoadData()
                : JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros)!;

            return _db.Sif_DocsTraslado_Desbalanceados_Obtener(CodEmpresa, filtros, fechaInicio, fechaFin);
        }

        public ErrorDto<List<SifDocsTrasladoDesbalanceadoData>> Sif_DocsTraslado_Desbalanceados_Export(int CodEmpresa,string jfiltros,DateTime fechaInicio,DateTime fechaFin)
        {
            var filtros = string.IsNullOrWhiteSpace(jfiltros)
                ? new FiltrosLazyLoadData()
                : JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros);

            return _db.Sif_DocsTraslado_Desbalanceados_Export(CodEmpresa, filtros!, fechaInicio, fechaFin);
        }

        public ErrorDto<SifDocsTrasladoDocumentoConfig> Sif_DocsTraslado_Documento_Config_Obtener(int CodEmpresa,string tipoDocumento)
        {
            return _db.Sif_DocsTraslado_Documento_Config_Obtener(CodEmpresa, tipoDocumento);
        }
        public ErrorDto<string> Sif_DocsTraslado_Reactivar(int CodEmpresa,DateTime fechaInicio,DateTime fechaFin)
        {
            return _db.Sif_DocsTraslado_Reactivar(CodEmpresa, fechaInicio, fechaFin);
        }

        public ErrorDto<string> Sif_DocsTraslado_Aplica(int CodEmpresa, string jrequest)
        {
            var dto = string.IsNullOrWhiteSpace(jrequest)
                ? new SifDocsTrasladoEjecutarRequest()
                : JsonConvert.DeserializeObject<SifDocsTrasladoEjecutarRequest>(jrequest);

            return _db.Sif_DocsTraslado_Aplica(CodEmpresa, dto ?? new SifDocsTrasladoEjecutarRequest());
        }

        public ErrorDto<SifDocsTrasladoResultadoLote> Sif_DocsTraslado_Aplica_Lote(int CodEmpresa, string jrequest)
        {
            var dto = string.IsNullOrWhiteSpace(jrequest)
                ? new SifDocsTrasladoEjecutarLoteRequest()
                : JsonConvert.DeserializeObject<SifDocsTrasladoEjecutarLoteRequest>(jrequest);

            return _db.Sif_DocsTraslado_Aplica_Lote(CodEmpresa, dto!);
        }
    }
}