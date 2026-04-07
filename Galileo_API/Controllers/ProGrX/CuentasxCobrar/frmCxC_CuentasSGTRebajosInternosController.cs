using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.CuentasxCobrar;
using Galileo_API.Models.ProGrX.CuentasxCobrar;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.CuentasxCobrar
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCxCCuentasSGTRebajosInternosController : ControllerBase
    {
        private readonly FrmCxCCuentasSGTRebajosInternosBL BL;

        public FrmCxCCuentasSGTRebajosInternosController(IConfiguration config)
        {
            BL = new FrmCxCCuentasSGTRebajosInternosBL(config);
        }

        
        [HttpGet("CxC_Cuentas_SGT_Rebajos_Operacion_Obtener")]
        public ErrorDto<CxCCuentasSGTRebajosInternosPantallaDto> CxC_Cuentas_SGT_Rebajos_Operacion_Obtener(
            int codEmpresa,
            int operacion)
        {
            return BL.CxC_Cuentas_SGT_Rebajos_Operacion_Obtener(codEmpresa, operacion);
        }

        [HttpGet("CxC_Cuentas_SGT_Rebajos_CargoReposicion_Obtener")]
        public ErrorDto<decimal> CxC_Cuentas_SGT_Rebajos_CargoReposicion_Obtener(
    int codEmpresa,
    int operacion)
        {
            return BL.CxC_Cuentas_SGT_Rebajos_CargoReposicion_Obtener(codEmpresa, operacion);
        }
    }
}
