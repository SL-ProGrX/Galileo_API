using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Inversiones;
using Galileo_API.BusinessLogic.ProGrX_Inversiones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Inversiones
{
    [Route("api/frmIVR_Cat_Emisores")]
    [Authorize]
    [ApiController]
    public sealed class FrmIvrCatEmisoresController : ControllerBase
    {
        private readonly FrmIvrCatEmisoresBl _bl;

        public FrmIvrCatEmisoresController(IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            _bl = new FrmIvrCatEmisoresBl(config);
        }

        [HttpGet("IVR_Cat_Emisores_Lista_Obtener")]
        public ErrorDto<List<IvrCatEmisorDto>> IVR_Cat_Emisores_Lista_Obtener(
            [FromQuery] int codEmpresa)
        {
            return _bl.IVR_Cat_Emisores_Lista_Obtener(codEmpresa);
        }

        [HttpGet("IVR_Cat_Emisores_Sectores_Obtener")]
        public ErrorDto<List<IvrCatEmisorSectorDto>> IVR_Cat_Emisores_Sectores_Obtener(
            [FromQuery] int codEmpresa)
        {
            return _bl.IVR_Cat_Emisores_Sectores_Obtener(codEmpresa);
        }

        [ValidateAntiForgeryToken]
        [HttpPost("IVR_Cat_Emisores_Registrar")]
        public ErrorDto IVR_Cat_Emisores_Registrar(
            [FromQuery] int codEmpresa,
            [FromBody] IvrCatEmisorGuardarRequest? request)
        {
            return _bl.IVR_Cat_Emisores_Registrar(codEmpresa, request);
        }

        [ValidateAntiForgeryToken]
        [HttpPut("IVR_Cat_Emisores_Actualizar")]
        public ErrorDto IVR_Cat_Emisores_Actualizar(
            [FromQuery] int codEmpresa,
            [FromBody] IvrCatEmisorGuardarRequest? request)
        {
            return _bl.IVR_Cat_Emisores_Actualizar(codEmpresa, request);
        }

        [ValidateAntiForgeryToken]
        [HttpDelete("IVR_Cat_Emisores_Eliminar")]
        public ErrorDto IVR_Cat_Emisores_Eliminar(
            [FromQuery] int codEmpresa,
            [FromQuery] string? codEmisor,
            [FromQuery] string? usuario)
        {
            return _bl.IVR_Cat_Emisores_Eliminar(codEmpresa, codEmisor, usuario);
        }
    }
}
