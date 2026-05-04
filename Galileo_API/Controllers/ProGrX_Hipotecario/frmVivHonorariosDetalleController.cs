using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Hipotecario;
using Galileo_API.Models.ProGrX_Hipotecario;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Hipotecario
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmVivHonorariosDetalleController : ControllerBase
    {
        private readonly FrmVivHonorariosDetalleBl _bl;

        public FrmVivHonorariosDetalleController(IConfiguration config)
        {
            _bl = new FrmVivHonorariosDetalleBl(config);
        }

        [HttpGet("VivHonorariosDetalle_ObtenerOperacion")]
        public ErrorDto<VivHonorariosDetalleOperacionData?> VivHonorariosDetalle_ObtenerOperacion(
            int codEmpresa, int operacion, int idContacto)
        {
            return _bl.VivHonorariosDetalle_ObtenerOperacion(codEmpresa, operacion, idContacto);
        }

        [HttpGet("VivHonorariosDetalle_ObtenerLineas")]
        public ErrorDto<List<VivHonorariosDetalleLineaData>> VivHonorariosDetalle_ObtenerLineas(int codEmpresa)
        {
            return _bl.VivHonorariosDetalle_ObtenerLineas(codEmpresa);
        }

        [HttpPost("VivHonorariosDetalle_Guardar")]
        public ErrorDto VivHonorariosDetalle_Guardar(
            int codEmpresa, string usuario, VivHonorariosDetalleGuardarRequest request)
        {
            return _bl.VivHonorariosDetalle_Guardar(codEmpresa, usuario, request);
        }
    }
}