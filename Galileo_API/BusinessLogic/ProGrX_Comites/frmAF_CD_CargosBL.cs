using Galileo.Models.ERROR;
using Galileo.Models;
using Galileo_API.DataBaseTier.ProGrX_Comites;  
using Newtonsoft.Json; 
using static Galileo_API.Models.ProGrX_Comites.FrmAfCdCargos;

namespace Galileo_API.BusinessLogic.ProGrX_Comites
{
    public class FrmAfCdCargosBL
    {
        private readonly FrmAfCdCargosDB _db;

        public FrmAfCdCargosBL(IConfiguration config)
        {
            _db = new FrmAfCdCargosDB(config);
        }

        public ErrorDto<CdCargosLista> CdCargosLista_Obtener(int codEmpresa, string jfiltros, bool esExportar)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.CdCargosLista_Obtener(codEmpresa, filtros, esExportar);
        }
        public ErrorDto AfCdCargos_Guardar(int codEmpresa, string usuario, CdCargosData datos)
                      => _db.AfCdCargos_Guardar(codEmpresa, usuario, datos);

        public ErrorDto AfCdCargos_Eliminar(int codEmpresa, string usuario, string CodCargo)
                  => _db.AfCdCargos_Eliminar(codEmpresa, usuario, CodCargo);
    }
}
