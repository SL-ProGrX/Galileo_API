using Galileo.Models.ERROR;
using Galileo.Models;
using Galileo_API.DataBaseTier.ProGrX_Comites;  
using Newtonsoft.Json;
using static Galileo_API.Models.ProGrX_Comites.FrmAfCdTiposEstados;

namespace Galileo_API.BusinessLogic.ProGrX_Comites
{
    public class FrmAfCdTiposEstadosBL
    {
        private readonly FrmAfCdTiposEstadosDB _db;

        public FrmAfCdTiposEstadosBL(IConfiguration config)
        {
            _db = new FrmAfCdTiposEstadosDB(config);
        }

        public ErrorDto<CdTiposEstadosLista> AfCdTiposEstadosLista_Obtener(int codEmpresa, string jfiltros, bool esExportar)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.AfCdTiposEstadosLista_Obtener(codEmpresa, filtros, esExportar);
        }


        public ErrorDto AfCdTiposEstados_Guardar(int codEmpresa, string usuario, CdTiposEstadosData datos)
                 => _db.AfCdTiposEstados_Guardar(codEmpresa, usuario, datos);

        public ErrorDto AfCdTiposEstados_Eliminar(int codEmpresa, string usuario, string CodTipoCuenta)
                   => _db.AfCdTiposEstados_Eliminar(codEmpresa, usuario, CodTipoCuenta);
    }
}
