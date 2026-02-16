using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX_Nucleo;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;

namespace Galileo.BusinessLogic.ProGrX_Nucleo
{
    public class FrmSifConceptosBL(IConfiguration config)
    {
        private readonly FrmSifConceptosDB _db = new(config);

        public ErrorDto<SifConceptoLista> SIF_ConceptosLista_Obtener(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.SIF_ConceptosLista_Obtener(CodEmpresa, filtros);
        }
        public ErrorDto<List<SifConceptoData>> SIF_Conceptos_Obtener(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.SIF_Conceptos_Obtener(CodEmpresa, filtros);
        }
        public ErrorDto SIF_Conceptos_Guardar(int CodEmpresa, string usuario, SifConceptoData concepto)
        {
            return _db.SIF_Conceptos_Guardar(CodEmpresa, usuario, concepto);
        }
        public ErrorDto SIF_Conceptos_Eliminar(int CodEmpresa, string usuario, string cod_concepto)
        {
            return _db.SIF_Conceptos_Eliminar(CodEmpresa, usuario, cod_concepto);
        }

        public ErrorDto SIF_Conceptos_Valida(int codempresa, string cod_concepto)
        {
            return _db.SIF_Conceptos_Valida(codempresa, cod_concepto);
        }
     
        public ErrorDto<List<SifConceptoDocumentoData>> SIF_ConceptosDocumentos_Obtener(int CodEmpresa, string cod_concepto)
        {
            return _db.SIF_ConceptosDocumentos_Obtener(CodEmpresa, cod_concepto);
        }
        public ErrorDto SIF_ConceptosDocumentos_Asociar(int CodEmpresa, string usuario, string cod_concepto, string tipo_documento)
        {
            return _db.SIF_ConceptosDocumentos_Asociar(CodEmpresa, usuario, cod_concepto, tipo_documento);
        }
 
        public ErrorDto SIF_ConceptosDocumentos_Desasociar(int CodEmpresa, string usuario, string cod_concepto, string tipo_documento)
        {
            return _db.SIF_ConceptosDocumentos_Desasociar(CodEmpresa, usuario, cod_concepto, tipo_documento);
        }
    }
}
