
using Galileo.Models;
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
    public class FrmCxCCuentasAnulacionesController : ControllerBase
    {
        private readonly FrmCxCCuentasAnulacionesBl _bl;

        public FrmCxCCuentasAnulacionesController(IConfiguration config) => _bl = new FrmCxCCuentasAnulacionesBl(config);

        [HttpGet("CxcOperacion_Obtener")]
        public ErrorDto<CxcOperacionAnulacionData?> CxcOperacion_Obtener(int codEmpresa, int operacion)
        {
            return _bl.CxcOperacion_Obtener(codEmpresa, operacion);
        }

        [HttpGet("CxcOperacionMovimientos_Lista_Obtener")]
        public ErrorDto<List<CxcOperacionMovimientoData>> CxcOperacionMovimientos_Lista_Obtener(int codEmpresa, int operacion)
        {
            return _bl.CxcOperacionMovimientos_Lista_Obtener(codEmpresa, operacion);
        }

        [HttpPost("CxcCuentasAbono_Anular")]
        public ErrorDto CxcCuentasAbono_Anular(int codEmpresa, CxcAbonoAnularParams req)
        {
            return _bl.CxcCuentasAbono_Anular(codEmpresa, req);
        }
    }
}