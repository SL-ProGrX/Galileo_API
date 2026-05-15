using Galileo.DataBaseTier.ProGrX.Fondos;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.BusinessLogic.ProGrX.Fondos
{
    public class FrmFndPlanesBl
    {
        private readonly FrmFndPlanesDb _Db;

        public FrmFndPlanesBl(IConfiguration config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            _Db = new FrmFndPlanesDb(config);
        }

        public ErrorDto<FndPlanesCombosDto> FND_Planes_Combos_Obtener(int CodEmpresa)
        {
            return _Db.FND_Planes_Combos_Obtener(CodEmpresa);
        }

        public ErrorDto<List<PlanEstadoDto>> Fnd_Planes_Estados_Obtener(int codEmpresa, int codOperadora, string codPlan)
        {
            return _Db.Fnd_Planes_Estados_Obtener(codEmpresa, codOperadora, codPlan);
        }

        public ErrorDto<List<PlanPlazoDto>> Fnd_Planes_Plazos_Obtener(int codEmpresa, int codOperadora, string codPlan)
        {
            return _Db.Fnd_Planes_Plazos_Obtener(codEmpresa, codOperadora, codPlan);
        }

        public ErrorDto<FndPlanDto> Fnd_Planes_Obtener(int codEmpresa, int codOperadora, string codPlan)
        {
            return _Db.Fnd_Planes_Obtener(codEmpresa, codOperadora, codPlan);
        }

        public ErrorDto<FndPlanDto> AF_Plan_Scroll_Obtener(int CodEmpresa, string cod_plan, int scrollCode)
        {
            return _Db.AF_Plan_Scroll_Obtener(CodEmpresa, cod_plan, scrollCode);
        }

        public ErrorDto<List<FndHistorialRendDto>> FND_Historial_Rend_Obtener(int CodEmpresa, string cod_plan)
        {
            return _Db.Fnd_Historial_Rend_Obtener(CodEmpresa, cod_plan);
        }

        public ErrorDto<List<FndPlanRetiroDto>> Fnd_Planes_Retiros_Obtener(int CodEmpresa, int codoperadora, string codplan)
        {
            return _Db.Fnd_Planes_Retiros_Obtener(CodEmpresa, codoperadora, codplan);
        }

        public ErrorDto<List<FndPlanesDestinoAhorroDto>> Fnd_Planes_DestinosAhorro_Obtener(int CodEmpresa, string CodPlan)
        {
            return _Db.Fnd_Planes_DestinosAhorro_Obtener(CodEmpresa, CodPlan);
        }

        public ErrorDto<List<FndDestinoAsociadoDto>> Fnd_Planes_DestinosAsociaos_Obtener(int CodEmpresa, int codoperadora, string codplan)
        {
            return _Db.Fnd_Planes_DestinosAsociaos_Obtener(CodEmpresa, codoperadora, codplan);
        }
        public ErrorDto<List<FndReglaTasaDto>> Fnd_ReglasTasas_List(int CodEmpresa, int codOperadora, string codPlan)
        {
            return _Db.Fnd_ReglasTasas_List(CodEmpresa, codOperadora, codPlan);
        }

        public ErrorDto<List<FndReglaTasaDetalleDto>> Fnd_ReglasTasas_Detalle_Obtener(int CodEmpresa, int codOperadora, string codPlan, int id_per_tasa)
        {
            return _Db.Fnd_ReglasTasas_Detalle_Obtener(CodEmpresa, codOperadora, codPlan, id_per_tasa);
        }

        public ErrorDto<FndPlanRetiroDto> Fnd_Planes_Retiros_Guardar(int CodEmpresa, string usuario, FndPlanRetiroDto dto)
        {
            var resp = new ErrorDto<FndPlanRetiroDto> { Code = 0 };

            // Validaciones tipo Primes / Ubicaciones
            if (dto.desde < 0 || dto.hasta < 0)
            {
                resp.Code = -2;
                resp.Description = "Los valores de 'Desde' y 'Hasta' deben ser mayores o iguales a 0.";
                return resp;
            }

            if (dto.porcentaje < 0)
            {
                resp.Code = -2;
                resp.Description = "La multa no puede ser negativa.";
                return resp;
            }

            if (string.IsNullOrEmpty(dto.aplicar))
            {
                resp.Code = -2;
                resp.Description = "Debe indicar el campo 'Aplicar'.";
                return resp;
            }

            return _Db.Fnd_Planes_Retiros_Guardar(CodEmpresa, usuario, dto);
        }

        public ErrorDto<string> Fnd_Planes_Retiros_Eliminar(int CodEmpresa, int id)
        {
            return _Db.Fnd_Planes_Retiros_Eliminar(CodEmpresa, id);
        }

        public ErrorDto<string> Fnd_Planes_Puntos_Eliminar(int codEmpresa, int id)
        {
            return _Db.Fnd_Planes_Puntos_Eliminar(codEmpresa, id);
        }

        public ErrorDto<FndPlanPuntoDto> Fnd_Planes_Puntos_Guardar(int CodEmpresa, string Usuario, FndPlanPuntoDto dto)
        {
            return _Db.Fnd_Planes_Puntos_Guardar(CodEmpresa, Usuario, dto);
        }

        public ErrorDto<FndPlanPuntoDetalleDto> Fnd_Planes_Puntos_Detalle_Guardar(int CodEmpresa, string Usuario, FndPlanPuntoDetalleDto dto)
        {
            return _Db.Fnd_Planes_Puntos_Detalle_Guardar(CodEmpresa, Usuario, dto);
        }

        public ErrorDto<string> Fnd_Planes_Puntos_Detalle_Eliminar(int CodEmpresa, int id)
        {
            return _Db.Fnd_Planes_Puntos_Detalle_Eliminar(CodEmpresa, id);
        }

        public ErrorDto Planes_Destinos_Guardar(int CodEmpresa, FndPlanDestinoGuardarDto dto)
        {
            return _Db.Planes_Destinos_Guardar(CodEmpresa, dto);
        }

        public ErrorDto<bool> Planes_Destinos_Eliminar(int CodEmpresa, int id, string usuario)
        {
            return _Db.Planes_Destinos_Eliminar(CodEmpresa, id, usuario);
        }

        public ErrorDto<bool> Fnd_Planes_Destinos_Asociados_Guardar(int CodEmpresa, string usuario, FndPlanDestinoAsociadoDto dto)
        {
            return _Db.Fnd_Planes_Destinos_Asociados_Guardar(CodEmpresa, usuario, dto);
        }

        public ErrorDto<bool> Fnd_Planes_Vencimientos_Guardar(int CodEmpresa, string usuario,
    FndPlanesVencimientosGuardarDto dto)
        {
            return _Db.Fnd_Planes_Vencimientos_Guardar(CodEmpresa, usuario, dto);
        }

        public ErrorDto Fnd_Reglas_Activar(int CodEmpresa, FndReglaActivarDto dto)
        {
            return _Db.Fnd_Reglas_Activar(CodEmpresa, dto);
        }

        public ErrorDto<FndPlanDto> Fnd_Plan_Guardar(int CodEmpresa, string usuario, FndPlanDto dto)
        {
            return _Db.Fnd_Plan_Guardar(CodEmpresa, usuario, dto);
        }

        public ErrorDto<FndPlanDto> Fnd_Plan_Eliminar(int CodEmpresa, string usuario, int codoperadora, string codplan)
        {
            return _Db.Fnd_Plan_Eliminar(CodEmpresa, usuario, codoperadora, codplan);
        }

        public ErrorDto<bool> Fnd_Plan_FechaCorte_Update(int CodEmpresa, string usuario, int codoperadora, string codplan, string fecha)
        {
            return _Db.Fnd_Plan_FechaCorte_Update(CodEmpresa, usuario, codoperadora, codplan, fecha);
        }

        public ErrorDto FechaServidor_Obtener(int CodEmpresa)
        {
            return _Db.FechaServidor_Obtener(CodEmpresa);
        }
    }
}