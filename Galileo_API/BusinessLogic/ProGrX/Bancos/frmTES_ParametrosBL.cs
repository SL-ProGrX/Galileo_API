using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.TES;
using Galileo_API.DataBaseTier.ProGrX.Bancos;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic
{
    public class FrmTesParametrosBL
    {
        private readonly FrmTesParametrosDB _parametrosDb;

        public FrmTesParametrosBL(IConfiguration config)
        {
            _parametrosDb = new FrmTesParametrosDB(config);
        }

        public ErrorDto<TablasListaGenericaModel> TES_Parametros_Obtener(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _parametrosDb.TES_Parametros_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto TES_Parametros_Guardar(int CodEmpresa, string Usuario, string Parametros)
        {
            TesParametrosDto param = JsonConvert.DeserializeObject<TesParametrosDto>(Parametros) ?? new TesParametrosDto();
            return _parametrosDb.TES_Parametros_Guardar(CodEmpresa, Usuario, param);
        }
    }
}