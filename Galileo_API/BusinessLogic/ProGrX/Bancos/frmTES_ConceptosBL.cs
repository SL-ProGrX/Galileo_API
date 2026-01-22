using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo_API.DataBaseTier.ProGrX.Bancos;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic.ProGrX.Bancos
{
    public class FrmTesConceptosBL
    {
        private readonly FrmTesConceptosDB _conceptosDb;

        public FrmTesConceptosBL(IConfiguration config)
        {
            _conceptosDb = new FrmTesConceptosDB(config);
        }

        public ErrorDto<TesConceptosLista> Tes_ConceptosLista_Obtener(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _conceptosDb.Tes_ConceptosLista_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto Tes_Conceptos_Guardar(int CodEmpresa, string usuario, TesConceptosData concepto)
        {
            return _conceptosDb.Tes_Conceptos_Guardar(CodEmpresa, usuario, concepto);
        }

        public ErrorDto Tes_Conceptos_Eliminar(int CodEmpresa, string tipo, string usuario)
        {
            return _conceptosDb.Tes_Conceptos_Eliminar(CodEmpresa, tipo, usuario);
        }

        public ErrorDto<List<TesConceptosData>> Tes_Conceptos_Obtener(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _conceptosDb.Tes_Conceptos_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto Tes_Concepto_Valida(int CodEmpresa, string codigo)
        {
            return _conceptosDb.Tes_Concepto_Valida(CodEmpresa, codigo);
        }
    }
}
