using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Inversiones;
using Galileo_API.BusinessLogic.ProGrX_Inversiones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Inversiones
{
    [Route("api/frmIVR_Cat_Parametros")]
    [Authorize]
    [ApiController]
    public sealed class FrmIvrCatParametrosController
        : ControllerBase
    {
        private readonly FrmIvrCatParametrosBl _bl;

        public FrmIvrCatParametrosController(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _bl = new FrmIvrCatParametrosBl(config);
        }

        [ValidateAntiForgeryToken]
        [HttpPost("IVR_Cat_Parametros_Inicializar")]
        public ErrorDto IVR_Cat_Parametros_Inicializar(
            [FromQuery] int codEmpresa)
        {
            return _bl.IVR_Cat_Parametros_Inicializar(
                codEmpresa);
        }

        [HttpGet("IVR_Cat_Parametros_Lista_Obtener")]
        public ErrorDto<List<IvrCatParametroDto>>
            IVR_Cat_Parametros_Lista_Obtener(
                [FromQuery] int codEmpresa)
        {
            return _bl.IVR_Cat_Parametros_Lista_Obtener(
                codEmpresa);
        }

        [ValidateAntiForgeryToken]
        [HttpPut("IVR_Cat_Parametros_Actualizar")]
        public ErrorDto IVR_Cat_Parametros_Actualizar(
            [FromQuery] int codEmpresa,
            [FromBody]
            IvrCatParametroActualizarRequest? request)
        {
            return _bl.IVR_Cat_Parametros_Actualizar(
                codEmpresa,
                request);
        }
    }
}