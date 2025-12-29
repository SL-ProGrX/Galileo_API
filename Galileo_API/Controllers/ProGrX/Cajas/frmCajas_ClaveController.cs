using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo_API.BusinessLogic.ProGrX.Cajas;
using Galileo.Models.ERROR;

using PgxAPI.Models.ProGrX.Cajas;

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
        public ErrorDto<List<CajasUsuarioDTO>> Cajas_Usuario_Obtener(int codEmpresa, string usuario)
        {
            return BL_Cajas_Clave.Cajas_Usuario_Obtener(codEmpresa, usuario);
        }
        [Authorize]
        [HttpPost("Cajas_Cambio_Clave")]
        public ErrorDto<bool> Cajas_Cambio_Clave(int codEmpresa, string usuario, string claveActual,
         string claveNueva, string cajas)
        {
            var listaCajas = string.IsNullOrWhiteSpace(cajas) ? new List<string>() : cajas.Split(',').Select(c => c.Trim()).ToList();

            return BL_Cajas_Clave.Cajas_Cambio_Clave(codEmpresa, usuario, claveActual, claveNueva, listaCajas);
        }
    }
}