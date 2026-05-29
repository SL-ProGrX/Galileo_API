using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX.Clientes;
using Galileo.Models;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic.ProGrX.Clientes
{
    public class FrmAfPromotoresReportesBL
    {
        private readonly FrmAfPromotoresReportesDB _db;
        public FrmAfPromotoresReportesBL(IConfiguration config)
        {
            _db = new FrmAfPromotoresReportesDB(config);
        }

        public ErrorDto<TablasListaGenericaModel> AF_PromotoresReportes_Obtener(int CodEmpresa, string filtro)
        {
            FiltrosLazyLoadData jfiltro = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(filtro) ?? new FiltrosLazyLoadData();
            return _db.AF_PromotoresReportes_Obtener(CodEmpresa, jfiltro);
        }
    }
}