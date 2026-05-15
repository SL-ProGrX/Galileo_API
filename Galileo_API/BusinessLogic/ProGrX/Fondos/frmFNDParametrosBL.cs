using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX.Fondos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.BusinessLogic.ProGrX.Fondos
{
    public class FrmFndParametrosBL
    {
        private readonly FrmFndParametrosDB _DB;
        public FrmFndParametrosBL(IConfiguration? config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            _DB = new FrmFndParametrosDB(config);
        }

        public ErrorDto<TablasListaGenericaModel> Fnd_Parametros_Obtener(int CodEmpresa, bool exporta, int cod_contabilidad, string strFiltro)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(strFiltro) ?? new FiltrosLazyLoadData();
            return _DB.Fnd_Parametros_Obtener(CodEmpresa, exporta, cod_contabilidad, filtros);
        }

        public ErrorDto Fnd_Parametros_Guardar(int CodEmpresa, string usuario, FndParametrosDto data)
        {
            return _DB.Fnd_Parametros_Guardar(CodEmpresa, usuario, data);
        }

    }
}