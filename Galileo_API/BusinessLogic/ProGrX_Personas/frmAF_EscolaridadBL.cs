using Galileo.Models.ERROR;
using Galileo.Models;
using Galileo_API.Models.ProGrX_Personas;
using Galileo_API.DataBaseTier.ProGrX_Personas;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic.ProGrX_Personas
{
    public class FrmAfEscolaridadBL
    {
        private readonly FrmAfEscolaridadDB _db;

        public FrmAfEscolaridadBL(IConfiguration config)
        {
            _db = new FrmAfEscolaridadDB(config);
        }
        
        public ErrorDto<NivelEscolaridadLista> AF_EscolaridadTipos_Obtener(int codEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData? filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros);
            return _db.AF_EscolaridadTipos_Obtener(codEmpresa, filtros);
        }

        public ErrorDto AF_EscolaridadTipos_Guardar(int codEmpresa, string usuario, NivelEscolaridadData escolaridad)
        {
            return _db.AF_EscolaridadTipos_Guardar(codEmpresa, usuario, escolaridad);
        }

        public ErrorDto AF_EscolaridadTipos_Eliminar(int codEmpresa, string usuario, string escolaridadTipo)
        {
            return _db.AF_EscolaridadTipos_Eliminar(codEmpresa, usuario, escolaridadTipo);
        }
        public ErrorDto AF_EscolaridadTipos_Valida(int codEmpresa, string escolaridadTipo)
        {
            return _db.AF_EscolaridadTipos_Valida(codEmpresa, escolaridadTipo);
        }

        public ErrorDto<NivelEscolaridadLista> AF_EscolaridadTipos_Exportar(int codEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData? filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros);
            return _db.AF_EscolaridadTipos_Exportar(codEmpresa, filtros);
        }
    }
}
