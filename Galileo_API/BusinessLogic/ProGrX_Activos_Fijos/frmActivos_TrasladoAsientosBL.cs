using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX_Activos_Fijos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;


namespace Galileo.BusinessLogic.ProGrX_Activos_Fijos
{
    public class FrmActivosTrasladoAsientosBL
    {
        private readonly FrmActivosTrasladoAsientosDB _db;

        public FrmActivosTrasladoAsientosBL(IConfiguration config)
        {
            _db = new FrmActivosTrasladoAsientosDB(config);
        }
        
        public ErrorDto<TablasListaGenericaModel> Activos_TrasladoAsientos_Lista_Obtener(int CodEmpresa, string jfiltros)
        {
            var filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.Activos_TrasladoAsientos_Lista_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto<bool> Activos_TrasladoAsientos_Trasladar(int CodEmpresa, List<ActivosTrasladoAsientoRequest> request)
        {
            return _db.Activos_TrasladoAsientos_Trasladar(CodEmpresa, request);
        }

    }
}