using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Fondos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.Controllers.ProGrX.Fondos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmFndNotificacionesController : ControllerBase
    {
        private readonly FrmFndNotificacionesBL _bl;
        public FrmFndNotificacionesController(IConfiguration? config)
        {
            _bl = new FrmFndNotificacionesBL(config);
        }

        [Authorize]
        [HttpGet("Fnd_Notificaciones_Operadora_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_Notificaciones_Operadora_Obtener(int CodEmpresa)
        {
            return _bl.Fnd_Notificaciones_Operadora_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("Fnd_Notificaciones_TipoMov_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_Notificaciones_TipoMov_Obtener(int CodEmpresa)
        {
            return _bl.Fnd_Notificaciones_TipoMov_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("Fnd_Notificaciones_Planes_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_Notificaciones_Planes_Obtener(int CodEmpresa, string operadora)
        {
            return _bl.Fnd_Notificaciones_Planes_Obtener(CodEmpresa, operadora);
        }

        [Authorize]
        [HttpGet("Fnd_Notificaciones_Plan_Obtener")]
        public ErrorDto<string> Fnd_Notificaciones_Plan_Obtener(int CodEmpresa, string operadora, string plan)
        {
            return _bl.Fnd_Notificaciones_Plan_Obtener(CodEmpresa, operadora, plan);
        }

        [Authorize]
        [HttpGet("Fnd_Notificaciones_Scroll_Obtener")]
        public ErrorDto<List<FndNotificacionData>> Fnd_Notificaciones_Scroll_Obtener(int codEmpresa, int codOperadora, string codPlanActual, bool siguiente)
        {
            return _bl.Fnd_Notificaciones_Scroll_Obtener(codEmpresa, codOperadora, codPlanActual, siguiente);
        }

        [Authorize]
        [HttpGet("Fnd_Notificaciones_ObtenerObtener")]
        public ErrorDto<FndNotificacionData> Fnd_Notificaciones_Obtener(int codEmpresa, string pNotifica)
        {
            return _bl.Fnd_Notificaciones_Obtener(codEmpresa, pNotifica);
        }

        [Authorize]
        [HttpGet("Fnd_Notificaciones_Lista")]
        public ErrorDto<List<FndNotificacionData>> Fnd_Notifica_List(int codEmpresa, int codOperadora, string codigo)
        {
            return _bl.Fnd_Notifica_List(codEmpresa, codOperadora, codigo);
        }

        [Authorize]
        [HttpPost("Fnd_Notificaciones_Guardar")]
        public ErrorDto<int> Fnd_Notificaciones_Guardar(int CodEmpresa, FndNotificacionData data)
        {
            return _bl.Fnd_Notificaciones_Guardar(CodEmpresa, data);
        }
    }
}