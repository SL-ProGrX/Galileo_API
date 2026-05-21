using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Personas;
using Galileo_API.DataBaseTier.ProGrX_Personas;
using Newtonsoft.Json;
using Galileo.Models;

namespace Galileo_API.BusinessLogic.ProGrX_Personas
{
    public class FrmAfPreferenciasTiposBL
    {
        private readonly FrmAfPreferenciasTiposDB _db;

        public FrmAfPreferenciasTiposBL(IConfiguration config)
        {
            _db = new FrmAfPreferenciasTiposDB(config);
        }

        public ErrorDto<PreferenciaTipoLista> AF_Preferencias_Obtener(int codEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData? filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros);
            return _db.AF_Preferencias_Obtener(codEmpresa, filtros);
        }

        public ErrorDto AF_Preferencias_Guardar(int codEmpresa, string usuario, PreferenciaTipoData preferenciaTipo)
        {
            return _db.AF_Preferencias_Guardar(codEmpresa, usuario, preferenciaTipo);
        }

        public ErrorDto AF_Preferencias_Eliminar(int codEmpresa, string usuario, string codPreferencia)
        {
            return _db.AF_Preferencias_Eliminar(codEmpresa, usuario, codPreferencia);
        }

        public ErrorDto AF_Preferencias_Valida(int codEmpresa, string codPreferencia)
        {
            return _db.AF_Preferencias_Valida(codEmpresa, codPreferencia);
        }

        public ErrorDto<PreferenciaTipoLista> AF_Preferencias_Exportar(int codEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData? filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros);
            return _db.AF_Preferencias_Exportar(codEmpresa, filtros);
        }
    }
}
