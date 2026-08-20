using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Conciliacion;
using Galileo_API.Models.ProGrX_Conciliacion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Conciliacion
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public sealed class FrmConCierreParcialesCreditosController : ControllerBase
    {
        private readonly FrmConCierreParcialesCreditosBL _bl;

        public FrmConCierreParcialesCreditosController(IConfiguration config)
        {
            _bl = new FrmConCierreParcialesCreditosBL(config);
        }

        [HttpGet("ConCierreParcialesCreditos_UltimoCorte_Obtener")]
        public ErrorDto<ConCierreParcialesCreditosUltimoCorteData?>
            ConCierreParcialesCreditos_UltimoCorte_Obtener(int codEmpresa)
        {
            return _bl.ConCierreParcialesCreditos_UltimoCorte_Obtener(codEmpresa);
        }

        [HttpPost("ConCierreParcialesCreditos_CierreParcial_Ejecutar")]
        public ErrorDto ConCierreParcialesCreditos_CierreParcial_Ejecutar(
            int codEmpresa,
            [FromBody] ConCierreParcialesCreditosCierreParcialRequest request)
        {
            return _bl.ConCierreParcialesCreditos_CierreParcial_Ejecutar(
                codEmpresa,
                request);
        }

        [HttpPost("ConCierreParcialesCreditos_ProyeccionCartera_Ejecutar")]
        public ErrorDto<List<Dictionary<string, object?>>>
            ConCierreParcialesCreditos_ProyeccionCartera_Ejecutar(
                int codEmpresa,
                [FromBody] ConCierreParcialesCreditosProyeccionRequest request)
        {
            return _bl.ConCierreParcialesCreditos_ProyeccionCartera_Ejecutar(
                codEmpresa,
                request);
        }

        [HttpPost("ConCierreParcialesCreditos_ProductoAcumulado_Ejecutar")]
        public ErrorDto ConCierreParcialesCreditos_ProductoAcumulado_Ejecutar(
            int codEmpresa,
            [FromBody] ConCierreParcialesCreditosCierreParcialRequest request)
        {
            return _bl.ConCierreParcialesCreditos_ProductoAcumulado_Ejecutar(
                codEmpresa,
                new ConCierreParcialesCreditosProductoAcumuladoRequest
                {
                    Fecha_Corte = request.Fecha_Corte,
                });
        }
    }
}
