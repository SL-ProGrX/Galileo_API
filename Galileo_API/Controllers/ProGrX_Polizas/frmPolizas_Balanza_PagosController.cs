using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Polizas;
using Galileo_API.Models.ProGrX_Polizas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Galileo_API.Controllers.ProGrX_Polizas
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmPolizasBalanzaPagosController : ControllerBase
    {
        private readonly FrmPolizasBalanzaPagosBL _bl;

        public FrmPolizasBalanzaPagosController(IConfiguration config)
        {
            _bl = new FrmPolizasBalanzaPagosBL(config);
        }

        [Authorize]
        [HttpGet("Polizas_Combo_Lista")]
        public ErrorDto<List<DropDownListaGenericaModel>> Polizas_Combo_Lista(int codEmpresa)
            => _bl.Polizas_Combo_Lista(codEmpresa);

        [Authorize]
        [HttpPost("Poliza_Informe_Balance_Pago_Resumen")]
        public ErrorDto<List<PolizaBalancePagoResumenDto>> Poliza_Informe_Balance_Pago_Resumen(int codEmpresa, [FromBody] PolizaBalancePagoParams param)
            => _bl.Poliza_Informe_Balance_Pago_Resumen(codEmpresa, param);

        [Authorize]
        [HttpPost("Poliza_Informe_Balance_Pago_Detalle")]
        public ErrorDto<List<PolizaBalancePagoDetalleDto>> Poliza_Informe_Balance_Pago_Detalle(int codEmpresa, [FromBody] PolizaBalancePagoParams param)
            => _bl.Poliza_Informe_Balance_Pago_Detalle(codEmpresa, param);
    }
}
