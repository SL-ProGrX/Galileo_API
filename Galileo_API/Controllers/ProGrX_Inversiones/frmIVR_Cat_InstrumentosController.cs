using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Inversiones;
using Galileo_API.BusinessLogic.ProGrX_Inversiones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Inversiones
{
    [Route("api/frmIVR_Cat_Instrumentos")]
    [Authorize]
    [ApiController]
    public sealed class FrmIvrCatInstrumentosController : ControllerBase
    {
        private readonly FrmIvrCatInstrumentosBl _bl;

        public FrmIvrCatInstrumentosController(IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _bl = new FrmIvrCatInstrumentosBl(config);
        }

        [HttpGet("IvrCatInstrumentos_Obtener")]
        public ErrorDto<List<IvrCatInstrumentoData>> IvrCatInstrumentos_Obtener(int codEmpresa)
            => _bl.IvrCatInstrumentos_Obtener(codEmpresa);

        [ValidateAntiForgeryToken]
        [HttpPost("IvrCatInstrumentos_Guardar")]
        public ErrorDto IvrCatInstrumentos_Guardar(int codEmpresa, IvrCatInstrumentoGuardarRequest request)
            => _bl.IvrCatInstrumentos_Guardar(codEmpresa, request);

        [ValidateAntiForgeryToken]
        [HttpDelete("IvrCatInstrumentos_Eliminar")]
        public ErrorDto IvrCatInstrumentos_Eliminar(int codEmpresa, IvrCatInstrumentoEliminarRequest request)
            => _bl.IvrCatInstrumentos_Eliminar(codEmpresa, request);
    }
}
