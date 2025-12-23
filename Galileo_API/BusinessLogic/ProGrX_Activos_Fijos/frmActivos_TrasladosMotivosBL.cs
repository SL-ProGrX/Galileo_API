using Galileo.DataBaseTier.ProGrX_Activos_Fijos;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;
using Galileo.Models;
using Newtonsoft.Json;

namespace Galileo.BusinessLogic.ProGrX_Activos_Fijos
{
    public class FrmActivosTrasladosMotivosBL
    {
        private readonly FrmActivosTrasladosMotivosDb _db;

        public FrmActivosTrasladosMotivosBL(IConfiguration config)
        {
            _db = new FrmActivosTrasladosMotivosDb(config);
        }

        public ErrorDto<ActivosTrasladosMotivosDataLista> Activos_TrasladosMotivos_Consultar(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.Activos_TrasladosMotivos_Consultar(CodEmpresa, filtros);
        }

        public ErrorDto<List<ActivosTrasladosMotivosData>> Activos_TrasladosMotivos_Obtener(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.Activos_TrasladosMotivos_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto Activos_TrasladosMotivos_Guardar(int CodEmpresa, string usuario, ActivosTrasladosMotivosData datos)
        {
            return _db.Activos_TrasladosMotivos_Guardar(CodEmpresa, usuario, datos);
        }

        public ErrorDto Activos_TrasladosMotivos_Eliminar(int CodEmpresa, string usuario, string cod_motivo)
        {
            return _db.Activos_TrasladosMotivos_Eliminar(CodEmpresa, usuario, cod_motivo);
        }
    }
}
