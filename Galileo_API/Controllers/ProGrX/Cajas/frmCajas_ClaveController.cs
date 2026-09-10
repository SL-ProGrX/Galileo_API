using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Galileo_API.BusinessLogic.ProGrX.Cajas;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Cajas;

namespace Galileo_API.Controllers.ProGrX.Cajas
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCajasClaveController : ControllerBase
    {
        private readonly FrmCajasClaveBl BL_Cajas_Clave;
        public FrmCajasClaveController(IConfiguration config)
        {
            BL_Cajas_Clave = new FrmCajasClaveBl(config);
        }
        [Authorize]
        [HttpGet("Cajas_Usuario_Obtener")]
        public ErrorDto<List<CajasUsuarioDto>> Cajas_Usuario_Obtener(int codEmpresa, string usuario)
        {
            return BL_Cajas_Clave.Cajas_Usuario_Obtener(codEmpresa, usuario);
        }
        [Authorize]
        [HttpPost("Cajas_Cambio_Clave")]
        public ErrorDto<bool> Cajas_Cambio_Clave([FromQuery] int codEmpresa,
            [FromBody] CajasCambioClaveRequestDto request)
        {
            // El token de perfil usa Name; el token inicial de login usa UserName.
            var usuario = User.FindFirst(ClaimTypes.Name)?.Value
                ?? User.FindFirst("UserName")?.Value;
            if (string.IsNullOrWhiteSpace(usuario) ||
                !string.Equals(usuario.Trim(), request.Usuario?.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return new ErrorDto<bool>
                {
                    Code = -1,
                    Description = "Solo puede cambiar las claves de caja de su usuario de sesión.",
                    Result = false
                };
            }

            var listaCajas = string.IsNullOrWhiteSpace(request.Cajas)
                ? new List<string>()
                : request.Cajas.Split(',').Select(c => c.Trim()).ToList();

            return BL_Cajas_Clave.Cajas_Cambio_Clave(codEmpresa, usuario.Trim(),
                request.ClaveActual, request.ClaveNueva, listaCajas);
        }
    }
}
