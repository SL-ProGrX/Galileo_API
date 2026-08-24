using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Conciliacion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Conciliacion
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public sealed class FrmCierreMensualAseController :
        ControllerBase
    {
        private readonly FrmCierreMensualAseBl _bl;

        public FrmCierreMensualAseController(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            _bl = new FrmCierreMensualAseBl(config);
        }

        [HttpPost(
            "Conciliacion_CierreMensualASE_Cierre_Ejecutar")]
        public ErrorDto
            Conciliacion_CierreMensualASE_Cierre_Ejecutar(
                int codEmpresa, string usuario)
        {
            return _bl
                .Conciliacion_CierreMensualASE_Cierre_Ejecutar(
                    codEmpresa,
                    usuario);
        }
    }
}