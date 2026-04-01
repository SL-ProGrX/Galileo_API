using Galileo.Models.ERROR;
using Galileo.Models;
using Galileo_API.DataBaseTier.ProGrX_Comites;
using static Galileo_API.Models.ProGrX_Comites.FrmAfCdTiposActividades;
using Galileo.Models.ProGrX_Nucleo;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic.ProGrX_Comites
{
    public class FrmAfCdTiposActividadesBL
    {
        private readonly FrmAfCdTiposActividadesDB _db;

        public FrmAfCdTiposActividadesBL(IConfiguration config)
        {
            _db = new FrmAfCdTiposActividadesDB(config);
        }

        public ErrorDto<CDTiposActividadesLista> AfCdTiposActividadesLista_Obtener(int codEmpresa, string jfiltros, bool esExportar)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.AfCdTiposActividadesLista_Obtener(codEmpresa, filtros, esExportar);
        }
              

        public ErrorDto AfCdTiposActividades_Guardar(int codEmpresa, string usuario, CDTiposActividadesData datos)
                 => _db.AfCdTiposActividades_Guardar(codEmpresa, usuario, datos);

        public ErrorDto AfCdTiposActividades_Eliminar(int codEmpresa, string usuario, string codTipoActividad)
                  => _db.AfCdTiposActividades_Eliminar(codEmpresa, usuario, codTipoActividad);
    }
}
