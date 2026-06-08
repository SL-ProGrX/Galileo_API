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
    public class FrmCrCargosAplicacionController : ControllerBase
    {
        private readonly FrmCrCargosAplicacionBl _bl;

        public FrmCrCargosAplicacionController(IConfiguration config)
        {
            _bl = new FrmCrCargosAplicacionBl(config);
        }

        [HttpGet("Cr_CargosAplicacion_Cargos_Obtener")]
        public ErrorDto<List<CrCargosAplicacionCargoData>> Cr_CargosAplicacion_Cargos_Obtener(int codEmpresa)
            => _bl.Cr_CargosAplicacion_Cargos_Obtener(codEmpresa);

        [HttpGet("Cr_CargosAplicacion_Operacion_Obtener")]
        public ErrorDto<CrCargosAplicacionOperacionData?> Cr_CargosAplicacion_Operacion_Obtener(
            int codEmpresa,
            int operacion)
            => _bl.Cr_CargosAplicacion_Operacion_Obtener(codEmpresa, operacion);

        [HttpPost("Cr_CargosAplicacion_Aplicar")]
        public ErrorDto Cr_CargosAplicacion_Aplicar(
            int codEmpresa,
            [FromBody] CrCargosAplicacionAplicarRequest request)
            => _bl.Cr_CargosAplicacion_Aplicar(codEmpresa, request);
    }
}