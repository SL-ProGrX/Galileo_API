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
    public class FrmCrRetencionDeduccionesController : ControllerBase
    {
        private readonly FrmCrRetencionDeduccionesBl _bl;

        public FrmCrRetencionDeduccionesController(IConfiguration config)
        {
            _bl = new FrmCrRetencionDeduccionesBl(config);
        }

        [HttpGet("Cr_RetencionDeducciones_Pantalla_Obtener")]
        public ErrorDto<CrRetencionDeduccionesPantallaData> Cr_RetencionDeducciones_Pantalla_Obtener(int codEmpresa)
            => _bl.Cr_RetencionDeducciones_Pantalla_Obtener(codEmpresa);

        [HttpPost("Cr_RetencionDeducciones_Obtener")]
        public ErrorDto<CrRetencionDeduccionesResultadoData> Cr_RetencionDeducciones_Obtener(
            int codEmpresa,
            [FromBody] CrRetencionDeduccionesObtenerRequest request)
            => _bl.Cr_RetencionDeducciones_Obtener(codEmpresa, request);

        [HttpPost("Cr_RetencionDeducciones_Archivo_Generar")]
        public ErrorDto<CrRetencionDeduccionesArchivoData> Cr_RetencionDeducciones_Archivo_Generar(
            int codEmpresa,
            [FromBody] CrRetencionDeduccionesArchivoRequest request)
            => _bl.Cr_RetencionDeducciones_Archivo_Generar(codEmpresa, request);
    }
}