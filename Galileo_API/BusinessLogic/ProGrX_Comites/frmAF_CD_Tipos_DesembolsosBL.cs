using Galileo.Models.ERROR;
using Galileo.Models;
using Galileo_API.DataBaseTier.ProGrX_Comites;  
using Newtonsoft.Json;
using static Galileo_API.Models.ProGrX_Comites.FrmAfCdTiposDesembolsos;

namespace Galileo_API.BusinessLogic.ProGrX_Comites
{
    public class FrmAfCdTiposDesembolsosBL
    {
        private readonly FrmAfCdTiposDesembolsosDB _db;

        public FrmAfCdTiposDesembolsosBL(IConfiguration config)
        {
            _db = new FrmAfCdTiposDesembolsosDB(config);
        }

        public ErrorDto<CdTiposDesembolsosLista> AfCdTiposDesembolsosLista_Obtener(int codEmpresa, string jfiltros, bool esExportar)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.AfCdTiposDesembolsosLista_Obtener(codEmpresa, filtros, esExportar);
        }


        public ErrorDto AfCdTiposDesembolsos_Guardar(int codEmpresa, string usuario, CdTiposDesembolsosData datos)
                 => _db.AfCdTiposDesembolsos_Guardar(codEmpresa, usuario, datos);

        public ErrorDto AfCdTiposDesembolsos_Eliminar(int codEmpresa, string usuario, string CodTipoCuenta)
                   => _db.AfCdTiposDesembolsos_Eliminar(codEmpresa, usuario, CodTipoCuenta);
    }
}
