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
    public class FrmFndSeguridadTasaPreferencialController : ControllerBase
    {
        private readonly FrmFndSeguridadTasaPreferencialBL _BL;

        public FrmFndSeguridadTasaPreferencialController(IConfiguration? config)
        {
            _BL = new FrmFndSeguridadTasaPreferencialBL(config);
        }

        [Authorize]
        [HttpGet("Fnd_SeguridadTasaPreferencial_Obtener")]
        public ErrorDto<TablasListaGenericaModel> Fnd_SeguridadTasaPreferencial_Obtener(int CodEmpresa, string jFiltros)
        {
            return _BL.Fnd_SeguridadTasaPreferencial_Obtener(CodEmpresa, jFiltros);
        }

        [Authorize]
        [HttpGet("Fnd_SeguridadTasaPreferencial_Planes_Obtener")]
        public ErrorDto<List<FndSeguridadTasaPreferenciaPlanData>> Fnd_SeguridadTasaPreferencial_Planes_Obtener(int CodEmpresa, string rol, string? filtro)
        {
            return _BL.Fnd_SeguridadTasaPreferencial_Planes_Obtener(CodEmpresa, rol, filtro);
        }

        [Authorize]
        [HttpGet("Fnd_SeguridadTasaPreferencial_Usuarios_Obtener")]
        public ErrorDto<List<FndSeguridadTasaPreferenciaUsuarioData>> Fnd_SeguridadTasaPreferencial_Usuarios_Obtener(int CodEmpresa, string rol, string? filtro)
        {
            return _BL.Fnd_SeguridadTasaPreferencial_Usuarios_Obtener(CodEmpresa, rol, filtro);
        }

        [Authorize]
        [HttpPost("Fnd_SeguridadTasaPreferencial_Guardar")]
        public ErrorDto Fnd_SeguridadTasaPreferencial_Guardar(int CodEmpresa, FndSeguridadTasaPreferencialDto row, string usuario)
        {
            return _BL.Fnd_SeguridadTasaPreferencial_Guardar(CodEmpresa, row, usuario);
        }

        [Authorize]
        [HttpDelete("Fnd_SeguridadTasaPreferencial_Eliminar")]
        public ErrorDto Fnd_SeguridadTasaPreferencial_Eliminar(int CodEmpresa, string tp_rol, string usuario)
        {
            return _BL.Fnd_SeguridadTasaPreferencial_Eliminar(CodEmpresa, tp_rol, usuario);
        }

        [Authorize]
        [HttpPost("Fnd_SeguridadTasaPreferencial_RolPlan_Actualizar")]
        public ErrorDto Fnd_SeguridadTasaPreferencial_RolPlan_Actualizar(int CodEmpresa, FndSeguridadTasaPreferencialRolPlanDto data)
        {
            return _BL.Fnd_SeguridadTasaPreferencial_RolPlan_Actualizar(CodEmpresa, data);
        }

        [Authorize]
        [HttpPost("Fnd_SeguridadTasaPreferencial_RolAutorizador_Actualizar")]
        public ErrorDto Fnd_SeguridadTasaPreferencial_RolAutorizador_Actualizar(int CodEmpresa, FndSeguridadTasaPreferencialRolAutorizadorDto data)
        {
            return _BL.Fnd_SeguridadTasaPreferencial_RolAutorizador_Actualizar(CodEmpresa, data);
        }
    }
}