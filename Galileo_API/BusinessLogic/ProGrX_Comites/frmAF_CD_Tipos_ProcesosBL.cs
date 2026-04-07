using Galileo.Models.ERROR;
using Galileo.Models;
using Galileo_API.DataBaseTier.ProGrX_Comites;  
using Newtonsoft.Json;
using static Galileo_API.Models.ProGrX_Comites.FrmAfCdTiposProcesos;

namespace Galileo_API.BusinessLogic.ProGrX_Comites
{
    public class FrmAfCdTiposProcesosBL
    {
        private readonly FrmAfCdTiposProcesosDB _db;

        public FrmAfCdTiposProcesosBL(IConfiguration config)
        {
            _db = new FrmAfCdTiposProcesosDB(config);
        }

        public ErrorDto<CdTiposProcesosLista> AfCdTiposProcesosLista_Obtener(int codEmpresa, string jfiltros, bool esExportar)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.AfCdTiposProcesosLista_Obtener(codEmpresa, filtros, esExportar);
        }


        public ErrorDto AfCdTiposProcesos_Guardar(int codEmpresa, string usuario, CdTiposProcesosData datos)
                 => _db.AfCdTiposProcesos_Guardar(codEmpresa, usuario, datos);

        public ErrorDto AfCdTiposProcesos_Eliminar(int codEmpresa, string usuario, string CodTipoCuenta)
                   => _db.AfCdTiposProcesos_Eliminar(codEmpresa, usuario, CodTipoCuenta);
    }
}
