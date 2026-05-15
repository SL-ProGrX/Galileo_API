using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX.Fondos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.BusinessLogic.ProGrX.Fondos
{
    public class FrmFndAutorizacionMovBL
    {
        private readonly FrmFndAutorizacionMovDB _db;

        public FrmFndAutorizacionMovBL(IConfiguration? config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            _db = new FrmFndAutorizacionMovDB(config);
        }

        public ErrorDto<TablasListaGenericaModel> Fnd_Autorizacion_Mov_Obtener(int CodEmpresa, bool exporta , string strData, string strFiltro)
        {
            FndAutorizacionMovFiltros data = JsonConvert.DeserializeObject<FndAutorizacionMovFiltros>(strData) ?? new FndAutorizacionMovFiltros();
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(strFiltro) ?? new FiltrosLazyLoadData();
            return _db.Fnd_Autorizacion_Mov_Obtener(CodEmpresa, exporta, data, filtros);
        }

        public ErrorDto Fnd_Autorizacion_Mov_Autoriza(int CodEmpresa, string pGestion, string pAutorizador, List<FndAutorizacionMovData> movimiento)
        {
            return _db.Fnd_Autorizacion_Mov_Autoriza(CodEmpresa, pGestion, pAutorizador, movimiento);
        }
    }
}