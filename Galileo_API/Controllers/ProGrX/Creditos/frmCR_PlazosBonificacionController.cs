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
    public class FrmCrPlazosBonificacionController : ControllerBase
    {
        private readonly FrmCrPlazosBonificacionBl _bl;

        public FrmCrPlazosBonificacionController(IConfiguration config)
        {
            _bl = new FrmCrPlazosBonificacionBl(config);
        }

        [HttpGet("CrPlazosBonificacion_Planes_Obtener")]
        public ErrorDto<List<CrPlazosBonificacionPlanData>> CrPlazosBonificacion_Planes_Obtener(int codEmpresa)
            => _bl.CrPlazosBonificacion_Planes_Obtener(codEmpresa);

        [HttpGet("CrPlazosBonificacion_Scroll_Obtener")]
        public ErrorDto<CrPlazosBonificacionDefinicionData?> CrPlazosBonificacion_Scroll_Obtener(
            int codEmpresa, int scroll, string codPlazoBono)
            => _bl.CrPlazosBonificacion_Scroll_Obtener(codEmpresa, scroll, codPlazoBono);

        [HttpGet("CrPlazosBonificacion_Definicion_Obtener")]
        public ErrorDto<CrPlazosBonificacionDefinicionData?> CrPlazosBonificacion_Definicion_Obtener(
            int codEmpresa,
            string cod_plazo_bono)
            => _bl.CrPlazosBonificacion_Definicion_Obtener(codEmpresa, cod_plazo_bono);

        [HttpPost("CrPlazosBonificacion_Definicion_Guardar")]
        public ErrorDto CrPlazosBonificacion_Definicion_Guardar(
            int codEmpresa,
            CrPlazosBonificacionDefinicionGuardarRequest request)
            => _bl.CrPlazosBonificacion_Definicion_Guardar(codEmpresa, request);

        [HttpDelete("CrPlazosBonificacion_Definicion_Eliminar")]
        public ErrorDto CrPlazosBonificacion_Definicion_Eliminar(
            int codEmpresa,
            CrPlazosBonificacionDefinicionEliminarRequest request)
            => _bl.CrPlazosBonificacion_Definicion_Eliminar(codEmpresa, request);

        [HttpGet("CrPlazosBonificacion_Bonificaciones_Obtener")]
        public ErrorDto<List<CrPlazosBonificacionBonificacionData>> CrPlazosBonificacion_Bonificaciones_Obtener(
            int codEmpresa,
            string cod_plazo_bono)
            => _bl.CrPlazosBonificacion_Bonificaciones_Obtener(codEmpresa, cod_plazo_bono);

        [HttpPost("CrPlazosBonificacion_Bonificaciones_Guardar")]
        public ErrorDto CrPlazosBonificacion_Bonificaciones_Guardar(
            int codEmpresa,
            CrPlazosBonificacionBonificacionGuardarRequest request)
            => _bl.CrPlazosBonificacion_Bonificaciones_Guardar(codEmpresa, request);

        [HttpDelete("CrPlazosBonificacion_Bonificaciones_Eliminar")]
        public ErrorDto CrPlazosBonificacion_Bonificaciones_Eliminar(
            int codEmpresa,
            CrPlazosBonificacionBonificacionEliminarRequest request)
            => _bl.CrPlazosBonificacion_Bonificaciones_Eliminar(codEmpresa, request);

        [HttpGet("CrPlazosBonificacion_Asignaciones_Obtener")]
        public ErrorDto<List<CrPlazosBonificacionAsignacionData>> CrPlazosBonificacion_Asignaciones_Obtener(
            int codEmpresa,
            string cod_plazo_bono)
            => _bl.CrPlazosBonificacion_Asignaciones_Obtener(codEmpresa, cod_plazo_bono);

        [HttpPost("CrPlazosBonificacion_Asignaciones_Guardar")]
        public ErrorDto CrPlazosBonificacion_Asignaciones_Guardar(
            int codEmpresa,
            CrPlazosBonificacionAsignacionGuardarRequest request)
            => _bl.CrPlazosBonificacion_Asignaciones_Guardar(codEmpresa, request);
    }
}