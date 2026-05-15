using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Fondos;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.Controllers.ProGrX.Fondos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmFndPlanesController : ControllerBase
    {
        private readonly FrmFndPlanesBl _BL;

        public FrmFndPlanesController(IConfiguration config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            _BL = new FrmFndPlanesBl(config);
        }

        [Authorize]
        [HttpGet("FND_Planes_Combos_Obtener")]
        public ErrorDto<FndPlanesCombosDto> FND_Planes_Combos_Obtener(int CodEmpresa)
        {
            return _BL.FND_Planes_Combos_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("Fnd_Planes_Estados_Obtener")]
        public ActionResult<ErrorDto<List<PlanEstadoDto>>> Fnd_Planes_Estados_Obtener(int CodEmpresa, int CodOperadora, string CodPlan)
        {
            return _BL.Fnd_Planes_Estados_Obtener(CodEmpresa, CodOperadora, CodPlan);
        }

        [Authorize]
        [HttpGet("Fnd_Planes_Plazos_Obtener")]
        public ActionResult<ErrorDto<List<PlanPlazoDto>>> Fnd_Planes_Plazos_Obtener(int CodEmpresa, int CodOperadora, string CodPlan)
        {
            return _BL.Fnd_Planes_Plazos_Obtener(CodEmpresa, CodOperadora, CodPlan);
        }

        [Authorize]
        [HttpGet("Fnd_Planes_Obtener")]
        public ActionResult<ErrorDto<FndPlanDto>> Fnd_Planes_Obtener(int CodEmpresa, int CodOperadora, string CodPlan)
        {
            return _BL.Fnd_Planes_Obtener(CodEmpresa, CodOperadora, CodPlan);
        }

        [Authorize]
        [HttpGet("FND_Plan_Scroll_Obtener")]
        public ErrorDto<FndPlanDto> AF_Operadora_Scroll_Obtener(int CodEmpresa, string cod_plan, int scrollCode)
        {
            return _BL.AF_Plan_Scroll_Obtener(CodEmpresa, cod_plan, scrollCode);
        }

        [Authorize]
        [HttpGet("FND_Historial_Rend_Obtener")]
        public ErrorDto<List<FndHistorialRendDto>> FND_Historial_Rend_Obtener(int CodEmpresa, string cod_plan)
        {
            return _BL.FND_Historial_Rend_Obtener(CodEmpresa, cod_plan);
        }

        [Authorize]
        [HttpGet("Fnd_Planes_Retiros_Obtener")]
        public ErrorDto<List<FndPlanRetiroDto>> Fnd_Planes_Retiros_Obtener(int CodEmpresa, int codoperadora, string codplan)
        {
            return _BL.Fnd_Planes_Retiros_Obtener(CodEmpresa, codoperadora, codplan);
        }

        [Authorize]
        [HttpGet("Fnd_Planes_DestinosAhorro_Obtener")]
        public ErrorDto<List<FndPlanesDestinoAhorroDto>> Fnd_Planes_DestinosAhorro_Obtener(int CodEmpresa, string codplan)
        {
            return _BL.Fnd_Planes_DestinosAhorro_Obtener(CodEmpresa, codplan);
        }

        [Authorize]
        [HttpGet("Fnd_Planes_Destinos_Asociados_Obtener")]
        public ErrorDto<List<FndDestinoAsociadoDto>> Fnd_Planes_Destinos_Asociados_Obtener(int CodEmpresa, int codoperadora, string codplan)
        {
            return _BL.Fnd_Planes_DestinosAsociaos_Obtener(CodEmpresa, codoperadora, codplan);
        }

        [Authorize]
        [HttpGet("Fnd_ReglasTasas_List")]
        public ErrorDto<List<FndReglaTasaDto>> Fnd_ReglasTasas_List(int CodEmpresa, int codOperadora, string codPlan)
        {
            return _BL.Fnd_ReglasTasas_List(CodEmpresa, codOperadora, codPlan);
        }

        [Authorize]
        [HttpGet("Fnd_ReglasTasas_Detalle_Obtener")]
        public ErrorDto<List<FndReglaTasaDetalleDto>> Fnd_ReglasTasas_Detalle_Obtener(int CodEmpresa, int codOperadora, string codPlan, int id_per_tasa)
        {
            return _BL.Fnd_ReglasTasas_Detalle_Obtener(CodEmpresa, codOperadora, codPlan, id_per_tasa);
        }

        [Authorize]
        [HttpPost("Fnd_Planes_Retiros_Guardar")]
        public ErrorDto<FndPlanRetiroDto> Fnd_Planes_Retiros_Guardar(
            int CodEmpresa, string usuario, [FromBody] FndPlanRetiroDto dto)
        {
            return _BL.Fnd_Planes_Retiros_Guardar(CodEmpresa, usuario, dto);
        }

        [Authorize]
        [HttpDelete("Fnd_Planes_Retiros_Eliminar")]
        public ErrorDto<string> Fnd_Planes_Retiros_Eliminar(
            int CodEmpresa, int id)
        {
            return _BL.Fnd_Planes_Retiros_Eliminar(CodEmpresa, id);
        }

        [Authorize]
        [HttpDelete("Fnd_Planes_Puntos_Eliminar")]
        public ErrorDto<string> Fnd_Planes_Puntos_Eliminar(int CodEmpresa, int id)
        {
            return _BL.Fnd_Planes_Puntos_Eliminar(CodEmpresa, id);
        }

        [Authorize]
        [HttpPost("Fnd_Planes_Puntos_Guardar")]
        public ErrorDto<FndPlanPuntoDto> Fnd_Planes_Puntos_Guardar(int CodEmpresa, string Usuario, FndPlanPuntoDto dto)
        {
            return _BL.Fnd_Planes_Puntos_Guardar(CodEmpresa, Usuario, dto);
        }

        [Authorize]
        [HttpPost("Fnd_Planes_Puntos_Detalle_Guardar")]
        public ErrorDto<FndPlanPuntoDetalleDto> Fnd_Planes_Puntos_Detalle_Guardar(int CodEmpresa, string Usuario, FndPlanPuntoDetalleDto dto)
        {
            return _BL.Fnd_Planes_Puntos_Detalle_Guardar(CodEmpresa, Usuario, dto);
        }

        [Authorize]
        [HttpDelete("Fnd_Planes_Puntos_Detalle_Eliminar")]
        public ErrorDto<string> Fnd_Planes_Puntos_Detalle_Eliminar(int CodEmpresa, int id)
        {
            return _BL.Fnd_Planes_Puntos_Detalle_Eliminar(CodEmpresa, id);
        }

        [Authorize]
        [HttpPost("Planes_Destinos_Guardar")]
        public ErrorDto Planes_Destinos_Guardar(int CodEmpresa, FndPlanDestinoGuardarDto dto)
        {
            return _BL.Planes_Destinos_Guardar(CodEmpresa, dto);
        }

        [Authorize]
        [HttpDelete("Planes_Destinos_Eliminar")]
        public ErrorDto<bool> Planes_Destinos_Eliminar(int CodEmpresa, int id, string usuario)
        {
            return _BL.Planes_Destinos_Eliminar(CodEmpresa, id, usuario);
        }

        [Authorize]
        [HttpPost("Fnd_Planes_Destinos_Asociados_Guardar")]
        public ErrorDto<bool> Fnd_Planes_Destinos_Asociados_Guardar(int CodEmpresa, string usuario, FndPlanDestinoAsociadoDto dto)
        {
            return _BL.Fnd_Planes_Destinos_Asociados_Guardar(CodEmpresa, usuario, dto);
        }

        [Authorize]
        [HttpPost("Fnd_Planes_Vencimientos_Guardar")]
        public ErrorDto<bool> Fnd_Planes_Vencimientos_Guardar(int CodEmpresa, string usuario, FndPlanesVencimientosGuardarDto dto)
        {
            return _BL.Fnd_Planes_Vencimientos_Guardar(CodEmpresa, usuario, dto);
        }

        [Authorize]
        [HttpPost("Fnd_Reglas_Activar")]
        public ErrorDto Fnd_Reglas_Activar(int CodEmpresa, [FromBody] FndReglaActivarDto dto)
        {
            return _BL.Fnd_Reglas_Activar(CodEmpresa, dto);
        }

        [Authorize]
        [HttpPost("Fnd_Plan_Guardar")]
        public ErrorDto<FndPlanDto> Fnd_Plan_Guardar(int CodEmpresa, string usuario, FndPlanDto dto)
        {
            return _BL.Fnd_Plan_Guardar(CodEmpresa, usuario, dto);
        }

        [Authorize]
        [HttpPost("Fnd_Plan_Eliminar")]
        public ErrorDto<FndPlanDto> Fnd_Plan_Eliminar(int CodEmpresa, string usuario, int codoperadora, string codplan)
        {
            return _BL.Fnd_Plan_Eliminar(CodEmpresa, usuario, codoperadora, codplan);
        }

        [Authorize]
        [HttpPost("Fnd_Plan_FechaCorte_Update")]
        public ErrorDto<bool> Fnd_Plan_FechaCorte_Update(int CodEmpresa, string usuario, int codoperadora, string codplan, string fecha)
        {
            return _BL.Fnd_Plan_FechaCorte_Update(CodEmpresa, usuario, codoperadora, codplan, fecha);
        }

        [Authorize]
        [HttpGet("FechaServidor_Obtener")]
        public ErrorDto FechaServidor_Obtener(int CodEmpresa)
        {
            return _BL.FechaServidor_Obtener(CodEmpresa);
        }
    }
}