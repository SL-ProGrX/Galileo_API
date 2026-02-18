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
    public class FrmCxCPlanPagosController : ControllerBase
    {
        private readonly FrmCxCPlanPagosBl _bl;

        public FrmCxCPlanPagosController(IConfiguration config) => _bl = new FrmCxCPlanPagosBl(config);

        [HttpGet("CxCPlanPagos_Operacion_Obtener")]
        public ErrorDto<CxCPlanPagosOperacionData> CxCPlanPagos_Operacion_Obtener(int codEmpresa, int operacionId)
        {
            return _bl.CxCPlanPagos_Operacion_Obtener(codEmpresa, operacionId);
        }

        [HttpGet("CxCPlanPagos_Movimientos_Obtener")]
        public ErrorDto<List<CxCPlanPagosMovimientoData>> CxCPlanPagos_Movimientos_Obtener(int codEmpresa, int operacionId)
        {
            return _bl.CxCPlanPagos_Movimientos_Obtener(codEmpresa, operacionId);
        }

        [HttpGet("CxCPlanPagos_ResumenOperacion_Obtener")]
        public ErrorDto<CxCPlanPagosOperacionResumenData> CxCPlanPagos_ResumenOperacion_Obtener(int codEmpresa, int operacionId)
        {
            return _bl.CxCPlanPagos_ResumenOperacion_Obtener(codEmpresa, operacionId);
        }
    }
}