using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Personas;
using Galileo_API.DataBaseTier.ProGrX_Personas;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic.ProGrX_Personas
{
    public class FrmAfNoCotizaRangosBL
    {
        private readonly FrmAfNoCotizaRangosDB _db;

        public FrmAfNoCotizaRangosBL(IConfiguration config)
        {
            _db = new FrmAfNoCotizaRangosDB(config);
        }

        public ErrorDto<NoCotizaRangosLista> AF_NoCotizaRangos_Obtener(int codEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData? filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros);
            return _db.AF_NoCotizaRangos_Obtener(codEmpresa, filtros);
        }

        public ErrorDto AF_NoCotizaRangos_Guardar(int codEmpresa, string usuario, NoCotizaRangosData rango)
        {
            return _db.AF_NoCotizaRangos_Guardar(codEmpresa, usuario, rango);
        }

        public ErrorDto AF_NoCotizaRangos_Eliminar(int codEmpresa, string usuario, int lineaId)
        {
            return _db.AF_NoCotizaRangos_Eliminar(codEmpresa, usuario, lineaId);
        }

        public ErrorDto<NoCotizaRangosLista> AF_NoCotizaRangos_Exportar(int codEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData? filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros);
            return _db.AF_NoCotizaRangos_Exportar(codEmpresa, filtros);
        }
    }
}
