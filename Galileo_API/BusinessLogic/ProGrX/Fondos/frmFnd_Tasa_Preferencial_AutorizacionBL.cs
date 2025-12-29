using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX.Fondos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.BusinessLogic.ProGrX.Fondos
{
    public class FrmFndTasaPreferencialAutorizacionBl
    {
        private readonly FrmFndTasaPreferencialAutorizacionDb _db;

        public FrmFndTasaPreferencialAutorizacionBl(IConfiguration config)
        {
            _db = new FrmFndTasaPreferencialAutorizacionDb(config);
        }

        public ErrorDto<TablasListaGenericaModel> Fnd_TasaPref_Obtener(int CodEmpresa, bool exporta, string strData, string strFiltro)
        {
            FndTasaPrefFiltros data = JsonConvert.DeserializeObject<FndTasaPrefFiltros>(strData) ?? new FndTasaPrefFiltros();
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(strFiltro) ?? new FiltrosLazyLoadData();
            return _db.Fnd_TasaPref_Obtener(CodEmpresa, exporta, data, filtros);
        }

        public ErrorDto Fnd_TasaPref_Autorizar(int CodEmpresa, string Gestion, string Autorizador, List<FndTPListDto> Gestiones)
        {
            return _db.Fnd_TasaPref_Autorizar(CodEmpresa, Gestion, Autorizador, Gestiones);
        }
    }
}
