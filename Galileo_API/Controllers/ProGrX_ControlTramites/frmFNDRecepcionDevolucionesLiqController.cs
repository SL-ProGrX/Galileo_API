using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_ControlTramites;
using Galileo_API.Models.ProGrX_ControlTramites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_ControlTramites
{
    [ApiController]
    [Route("api/FrmFNDRecepcionDevolucionesLiq")]
    [Authorize]
    public sealed class FrmFndRecepcionDevolucionesLiqController : ControllerBase
    {
        private readonly FrmFndRecepcionDevolucionesLiqBl _bl;

        public FrmFndRecepcionDevolucionesLiqController(IConfiguration config)
        {
            _bl = new FrmFndRecepcionDevolucionesLiqBl(config);
        }

        [HttpGet("FND_frmFNDRecepcionDevolucionesLiq_Inicializar")]
        public ErrorDto<FndRecepcionDevolucionesLiqInicializarData>
            FND_frmFNDRecepcionDevolucionesLiq_Inicializar(int codEmpresa)
        {
            return _bl.FND_frmFNDRecepcionDevolucionesLiq_Inicializar(
                codEmpresa);
        }

        [HttpGet("FND_frmFNDRecepcionDevolucionesLiq_Boleta_Obtener")]
        public ErrorDto<FndRecepcionDevolucionesLiqData?>
            FND_frmFNDRecepcionDevolucionesLiq_Boleta_Obtener(
                int codEmpresa,
                long numeroBoleta)
        {
            return _bl.FND_frmFNDRecepcionDevolucionesLiq_Boleta_Obtener(
                codEmpresa,
                numeroBoleta);
        }

        [HttpPost("FND_frmFNDRecepcionDevolucionesLiq_Aplicar")]
        public ErrorDto<FndRecepcionDevolucionesLiqAplicarData>
            FND_frmFNDRecepcionDevolucionesLiq_Aplicar(
                int codEmpresa,
                FndRecepcionDevolucionesLiqAplicarRequest request)
        {
            return _bl.FND_frmFNDRecepcionDevolucionesLiq_Aplicar(
                codEmpresa,
                request);
        }
    }
}
