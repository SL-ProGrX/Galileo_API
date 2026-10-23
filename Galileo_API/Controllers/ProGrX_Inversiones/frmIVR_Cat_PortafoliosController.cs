using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Inversiones;
using Galileo_API.BusinessLogic.ProGrX_Inversiones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Inversiones
{
    [Route("api/frmIVR_Cat_Portafolios")]
    [Authorize]
    [ApiController]
    public sealed class FrmIvrCatPortafoliosController : ControllerBase
    {
        private readonly FrmIvrCatPortafoliosBl _bl;

        public FrmIvrCatPortafoliosController(IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            _bl = new FrmIvrCatPortafoliosBl(config);
        }

        [HttpGet("IVR_Cat_Portafolios_Lista_Obtener")]
        public ErrorDto<List<IvrCatPortafolioDto>> IVR_Cat_Portafolios_Lista_Obtener(
            [FromQuery] int codEmpresa)
        {
            return _bl.IVR_Cat_Portafolios_Lista_Obtener(codEmpresa);
        }

        [HttpGet("IVR_Cat_Portafolios_Administradores_Obtener")]
        public ErrorDto<List<IvrCatPortafolioAdministradorDto>> IVR_Cat_Portafolios_Administradores_Obtener(
            [FromQuery] int codEmpresa,
            [FromQuery] string? codPortafolio)
        {
            return _bl.IVR_Cat_Portafolios_Administradores_Obtener(codEmpresa, codPortafolio);
        }

        [ValidateAntiForgeryToken]
        [HttpPost("IVR_Cat_Portafolios_Registrar")]
        public ErrorDto IVR_Cat_Portafolios_Registrar(
            [FromQuery] int codEmpresa,
            [FromBody] IvrCatPortafolioGuardarRequest? request)
        {
            return _bl.IVR_Cat_Portafolios_Registrar(codEmpresa, request);
        }

        [ValidateAntiForgeryToken]
        [HttpPut("IVR_Cat_Portafolios_Actualizar")]
        public ErrorDto IVR_Cat_Portafolios_Actualizar(
            [FromQuery] int codEmpresa,
            [FromBody] IvrCatPortafolioGuardarRequest? request)
        {
            return _bl.IVR_Cat_Portafolios_Actualizar(codEmpresa, request);
        }

        [ValidateAntiForgeryToken]
        [HttpDelete("IVR_Cat_Portafolios_Eliminar")]
        public ErrorDto IVR_Cat_Portafolios_Eliminar(
            [FromQuery] int codEmpresa,
            [FromQuery] string? codPortafolio,
            [FromQuery] string? usuario)
        {
            return _bl.IVR_Cat_Portafolios_Eliminar(codEmpresa, codPortafolio, usuario);
        }
    }
}
