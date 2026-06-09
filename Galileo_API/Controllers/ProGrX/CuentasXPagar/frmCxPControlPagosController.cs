using Microsoft.AspNetCore.Mvc;
using Galileo.Models.CxP;
using Galileo.Models.ERROR;
using Galileo.BusinessLogic.ProGrX.CxP;
using Microsoft.AspNetCore.Authorization;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class FrmCxPControlPagosController : ControllerBase
    {

       private readonly FrmCxPControlPagosBL _bl;
        public FrmCxPControlPagosController(IConfiguration config)
        {
            _bl = new FrmCxPControlPagosBL(config);
        }

        [HttpPost("CxPControlPagos_Obtener")]
        public ErrorDto<List<ControlPagosData>> CxPControlPagos_Obtener(int CodEmpresa, CxPControlPagosParametros pagosParametros)
        {
            return _bl.CxPControlPagos_Obtener(CodEmpresa, pagosParametros);
        }

        [HttpPost("CxPControlPagos_Resumen")]
        public ErrorDto<List<ControlPagosResumenData>> CxPControlPagos_Resumen(int CodEmpresa, CxPControlPagosParametros pagosParametros)
        {
            return _bl.CxPControlPagos_Resumen(CodEmpresa, pagosParametros);
        }
    }
}
