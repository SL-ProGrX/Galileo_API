using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Personas;
using Galileo_API.DataBaseTier.ProGrX_Personas;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic.ProGrX_Personas
{
    public class FrmAfPerfilTransaccionalBL
    {
        private readonly FrmAfPerfilTransaccionalDB _db;

        public FrmAfPerfilTransaccionalBL(IConfiguration config)
        {
            _db = new FrmAfPerfilTransaccionalDB(config);
        }

        public ErrorDto<PerfilTransaccionalLista> AF_PerfilTransaccional_Obtener(int codEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData? filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros);
            return _db.AF_PerfilTransaccional_Obtener(codEmpresa, filtros);
        }

        public ErrorDto AF_PerfilTransaccional_Guardar(int codEmpresa, string usuario, PerfilTransaccionalData perfil)
        {
            return _db.AF_PerfilTransaccional_Guardar(codEmpresa, usuario, perfil);
        }

        public ErrorDto AF_PerfilTransaccional_Eliminar(int codEmpresa, string usuario, int ptId)
        {
            return _db.AF_PerfilTransaccional_Eliminar(codEmpresa, usuario, ptId);
        }

        public ErrorDto<PerfilTransaccionalLista> AF_PerfilTransaccional_Exportar(int codEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData? filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros);
            return _db.AF_PerfilTransaccional_Exportar(codEmpresa, filtros);
        }
    }
}
