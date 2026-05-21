using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Personas;
using Galileo_API.DataBaseTier.ProGrX_Personas;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic.ProGrX_Personas
{
    public class FrmAfEstadoLaboralBL
    {
        private readonly FrmAfEstadoLaboralDB _db;

        public FrmAfEstadoLaboralBL(IConfiguration config)
        {
            _db = new FrmAfEstadoLaboralDB(config);
        }

        public ErrorDto<EstadoLaboralLista> AF_EstadoLaboral_Obtener(int codEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData? filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros);
            return _db.AF_EstadoLaboral_Obtener(codEmpresa, filtros);
        }

        public ErrorDto AF_EstadoLaboral_Guardar(int codEmpresa, string usuario, EstadoLaboralData estado)
        {
            return _db.AF_EstadoLaboral_Guardar(codEmpresa, usuario, estado);
        }

        public ErrorDto AF_EstadoLaboral_Eliminar(int codEmpresa, string usuario, string estadoLaboral)
        {
            return _db.AF_EstadoLaboral_Eliminar(codEmpresa, usuario, estadoLaboral);
        }

        public ErrorDto AF_EstadoLaboral_Valida(int codEmpresa, string estadoLaboral)
        {
            return _db.AF_EstadoLaboral_Valida(codEmpresa, estadoLaboral);
        }

        public ErrorDto<EstadoLaboralLista> AF_EstadoLaboral_Exportar(int codEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData? filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros);
            return _db.AF_EstadoLaboral_Exportar(codEmpresa, filtros);
        }
    }
}
