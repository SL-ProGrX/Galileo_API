using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public sealed class FrmInvVentasEnLineaSincController
        : ControllerBase
    {
        private readonly FrmInvVentasEnLineaSincBl _bl;

        public FrmInvVentasEnLineaSincController(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _bl = new FrmInvVentasEnLineaSincBl(config);
        }

        [HttpPost(
            "INV_VentasEnLineaSinc_CatalogoExistencias_Sincronizar")]
        public ErrorDto<InvVentasEnLineaSincData>
            INV_VentasEnLineaSinc_CatalogoExistencias_Sincronizar(
                int codEmpresa,
                [FromBody] InvVentasEnLineaSincRequest? request)
        {
            return _bl
                .INV_VentasEnLineaSinc_CatalogoExistencias_Sincronizar(
                    codEmpresa,
                    request);
        }

        [HttpPost(
            "INV_VentasEnLineaSinc_VentasExistencias_Sincronizar")]
        public ErrorDto<InvVentasEnLineaSincData>
            INV_VentasEnLineaSinc_VentasExistencias_Sincronizar(
                int codEmpresa,
                [FromBody] InvVentasEnLineaSincRequest? request)
        {
            return _bl
                .INV_VentasEnLineaSinc_VentasExistencias_Sincronizar(
                    codEmpresa,
                    request);
        }
    }
}