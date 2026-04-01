using Galileo.Models.ERROR;
using Galileo.Models;
using Galileo_API.DataBaseTier.ProGrX_Comites;  
using Newtonsoft.Json;
using static Galileo_API.Models.ProGrX_Comites.FrmAfCdTiposAprobaciones;

namespace Galileo_API.BusinessLogic.ProGrX_Comites
{
    public class FrmAfCdTiposAprobacionesBL
    {
        private readonly FrmAfCdTiposAprobacionesDB _db;

        public FrmAfCdTiposAprobacionesBL(IConfiguration config)
        {
            _db = new FrmAfCdTiposAprobacionesDB(config);
        }

        public ErrorDto<CdTiposAprobacionesLista> AfCdTiposAprobacionesLista_Obtener(int codEmpresa, string jfiltros, bool esExportar)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.AfCdTiposAprobacionesLista_Obtener(codEmpresa, filtros, esExportar);
        }


        public ErrorDto AfCdTiposAprobaciones_Guardar(int codEmpresa, string usuario, CdTiposAprobacionesData datos)
                 => _db.AfCdTiposAprobaciones_Guardar(codEmpresa, usuario, datos);

        public ErrorDto AfCdTiposAprobaciones_Eliminar(int codEmpresa, string usuario, string CodTipoAprobacion)
                   => _db.AfCdTiposAprobaciones_Eliminar(codEmpresa, usuario, CodTipoAprobacion);
    }
}
