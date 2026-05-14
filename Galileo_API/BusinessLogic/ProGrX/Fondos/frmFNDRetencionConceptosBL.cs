using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX.Fondos;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.BusinessLogic.ProGrX.Fondos
{
    public class FrmFndRetencionConceptosBL
    {
        private readonly FrmFndRetencionConceptosDB _db;
        public FrmFndRetencionConceptosBL(IConfiguration config)
        {
            _db = new FrmFndRetencionConceptosDB(config);
        }
        
        public ErrorDto<FndRetencionConceptoLista> FND_RetencionConceptosLista_Obtener(int CodEmpresa, string enlace, string jfiltros)
        {
            Models.FiltrosLazyLoadData? filtros = JsonConvert.DeserializeObject<Models.FiltrosLazyLoadData>(jfiltros);
            if (filtros == null)
            {
                throw new ArgumentNullException(nameof(jfiltros), "Deserialized filtros is null.");
            }
            return _db.FND_RetencionConceptosLista_Obtener(CodEmpresa, enlace, filtros);
        }

        public ErrorDto<List<FndRetencionConceptoData>> FND_RetencionConceptos_Obtener(int CodEmpresa, string enlace, string jfiltros)
        {
            Models.FiltrosLazyLoadData? filtros = JsonConvert.DeserializeObject<Models.FiltrosLazyLoadData>(jfiltros);
            if (filtros == null)
            {
                throw new ArgumentNullException(nameof(jfiltros), "Deserialized filtros is null.");
            }
            return _db.FND_RetencionConceptos_Obtener(CodEmpresa, enlace, filtros);
        }

        public ErrorDto FND_RetencionConceptos_Guardar(int CodEmpresa, string usuario, FndRetencionConceptoData concepto)
        {
            return _db.FND_RetencionConceptos_Guardar(CodEmpresa, usuario, concepto);
        }

        public ErrorDto FND_RetencionConceptos_Eliminar(int CodEmpresa, string usuario, string retencionCodigo)
        {
            return _db.FND_RetencionConceptos_Eliminar(CodEmpresa, usuario, retencionCodigo);
        }
        public ErrorDto FND_RetencionConceptos_Valida(int CodEmpresa, string retencionCodigo)
        {
            return _db.FND_RetencionConceptos_Valida(CodEmpresa, retencionCodigo);
        }
    }
}
