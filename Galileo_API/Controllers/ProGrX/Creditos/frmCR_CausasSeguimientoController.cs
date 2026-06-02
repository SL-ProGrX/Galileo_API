using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Credito;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCrCausasSeguimientoController : ControllerBase
    {
        private readonly FrmCrCausasSeguimientoBl _bl;

        public FrmCrCausasSeguimientoController(IConfiguration config)
        {
            _bl = new FrmCrCausasSeguimientoBl(config);
        }

        [HttpGet("CrCausasSeguimiento_Causas_Obtener")]
        public ErrorDto<List<CrCausasSeguimientoData>> CrCausasSeguimiento_Causas_Obtener(
            int codEmpresa, string tipo)
            => _bl.CrCausasSeguimiento_Causas_Obtener(codEmpresa, tipo);

        [HttpPost("CrCausasSeguimiento_Causas_Guardar")]
        public ErrorDto CrCausasSeguimiento_Causas_Guardar(
            int codEmpresa, CrCausasSeguimientoGuardarRequest request)
            => _bl.CrCausasSeguimiento_Causas_Guardar(codEmpresa, request);

        [HttpDelete("CrCausasSeguimiento_Causas_Eliminar")]
        public ErrorDto CrCausasSeguimiento_Causas_Eliminar(
            int codEmpresa, CrCausasSeguimientoEliminarRequest request)
            => _bl.CrCausasSeguimiento_Causas_Eliminar(codEmpresa, request);
    }
}