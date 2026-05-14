using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Fondos;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.Controllers.ProGrX.Fondos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmFndPlazosFrecuenciasController : ControllerBase
    {
        private readonly FrmFndPlazosFrecuenciasBl _bl;

        public FrmFndPlazosFrecuenciasController(IConfiguration config)
        {
            _bl = new FrmFndPlazosFrecuenciasBl(config);
        }

        [Authorize]
        [HttpGet("PlazosVencimiento_Obtener")]
        public ErrorDto<List<FndPlazoVencimientoModel>> PlazosVencimiento_Obtener(int codEmpresa)
        {
            return _bl.PlazosVencimiento_Obtener(codEmpresa);
        }

        [Authorize]
        [HttpPost("PlazosVencimiento_Guardar")]
        public ErrorDto<FndPlazoVencimientoSaveResult> PlazosVencimiento_Guardar(int codEmpresa, string usuario, [FromBody] FndPlazoVencimientoModel plazo, [FromQuery] string mov)
        {
            return _bl.PlazosVencimiento_Guardar(codEmpresa, usuario, plazo, mov);
        }

        [Authorize]
        [HttpDelete("PlazosVencimiento_Eliminar")]
        public ErrorDto<FndPlazoVencimientoSaveResult> PlazosVencimiento_Eliminar(int codEmpresa, int idPlazo, string usuario)
        {
            return _bl.PlazosVencimiento_Eliminar(codEmpresa, idPlazo, usuario);
        }

        [Authorize]
        [HttpGet("FrecuenciaCupon_Obtener")]
        public ErrorDto<List<FndFrecuenciaCuponModel>> FrecuenciaCupon_Obtener(int codEmpresa)
        {
            return _bl.FrecuenciaCupon_Obtener(codEmpresa);
        }

        [Authorize]
        [HttpPost("FrecuenciaCupon_Guardar")]
        public ErrorDto<FndFrecuenciaCuponSaveResult> FrecuenciaCupon_Guardar(int codEmpresa, string usuario, [FromBody] FndFrecuenciaCuponModel frecuencia, [FromQuery] string mov)
        {
            return _bl.FrecuenciaCupon_Guardar(codEmpresa, usuario, frecuencia, mov);
        }

        [Authorize]
        [HttpDelete("FrecuenciaCupon_Eliminar")]
        public ErrorDto<FndFrecuenciaCuponSaveResult> FrecuenciaCupon_Eliminar(int codEmpresa, int idFrecuenciaCupon, string usuario)
        {
            return _bl.FrecuenciaCupon_Eliminar(codEmpresa, idFrecuenciaCupon, usuario);
        }
    }
}