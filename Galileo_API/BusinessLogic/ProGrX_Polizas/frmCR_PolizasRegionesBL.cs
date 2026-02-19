using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Polizas;
using Galileo_API.Models.ProGrX_Polizas;

namespace Galileo_API.BusinessLogic.ProGrX_Polizas
{
    public class FrmCrPolizasRegionesBL
    {
        private readonly FrmCrPolizasRegionesDB _db;
        public FrmCrPolizasRegionesBL(IConfiguration config)
        {
            _db = new FrmCrPolizasRegionesDB(config);
        }

        public ErrorDto<List<CrdPolizasRegionDto>> Crd_Polizas_Region_Obtener(int CodEmpresa, string cod_poliza)
        {
            return _db.Crd_Polizas_Region_Obtener(CodEmpresa, cod_poliza);
        }
    }
}
