using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Polizas;
using Galileo_API.DataBaseTier.ProGrX_Polizas;
using Galileo_API.Models.ProGrX_Polizas;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Polizas
{
    public class FrmCrPolizasRegionesController : ControllerBase
    {
        private readonly FrmCrPolizasRegionesBL _dl;

        public FrmCrPolizasRegionesController(IConfiguration config)
        {
            _dl = new FrmCrPolizasRegionesBL(config);
        }

        [HttpGet("Crd_Polizas_Region_Obtener")]
        public ErrorDto<List<CrdPolizasRegionDto>> Crd_Polizas_Region_Obtener(int CodEmpresa, string cod_poliza)
        {
            return _dl.Crd_Polizas_Region_Obtener(CodEmpresa, cod_poliza);
        }

    }
}
