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
    public class FrmFndAlertasParametrosController : ControllerBase
    {
        private readonly FrmFndAlertasParametrosBL _bl;

        public FrmFndAlertasParametrosController(IConfiguration config)
        {
            _bl = new FrmFndAlertasParametrosBL(config);
        }

        [Authorize]
        [HttpGet("Fnd_AlertasParametros_Operadora_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_AlertasParametros_Operadora_Obtener(int CodEmpresa)
        {
            return _bl.Fnd_AlertasParametros_Operadora_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("Fnd_AlertasParametros_Planes_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_AlertasParametros_Planes_Obtener(int CodEmpresa, string operadora)
        {
            return _bl.Fnd_AlertasParametros_Planes_Obtener(CodEmpresa, operadora);
        }

        [Authorize]
        [HttpGet("Fnd_AlertasParametros_Plan_Obtener")]
        public ErrorDto<string> Fnd_AlertasParametros_Plan_Obtener(int CodEmpresa, string operadora, string plan)
        {
            return _bl.Fnd_AlertasParametros_Plan_Obtener(CodEmpresa, operadora, plan);
        }


        #region ALERTAS

        [Authorize]
        [HttpGet("Fnd_AlertasPlanes_Scroll_Obtener")]
        public ErrorDto<FndalertasData> Fnd_AlertasPlanes_Scroll_Obtener(int codEmpresa, int codOperadora, string codPlanActual, bool siguiente)
        {
            return _bl.Fnd_AlertasPlanes_Scroll_Obtener(codEmpresa, codOperadora, codPlanActual, siguiente);
        }

        [Authorize]
        [HttpGet("Fnd_AlertasParametros_Alerta_Obtener")]
        public ErrorDto<FndalertasData> Fnd_AlertasParametros_Alerta_Obtener(int CodEmpresa, int CodOperadora, string CodPlan)
        {
            return _bl.Fnd_AlertasParametros_Alerta_Obtener(CodEmpresa, CodOperadora, CodPlan);
        }

        [Authorize]
        [HttpGet("Fnd_AlertasParametros_Lista_Obtener")]
        public ErrorDto<TablasListaGenericaModel> Fnd_AlertasParametros_Lista_Obtener(int CodEmpresa, string jFiltros)
        {
            return _bl.Fnd_AlertasParametros_Lista_Obtener(CodEmpresa, jFiltros);
        }

        [Authorize]
        [HttpGet("Fnd_AlertasParametros_NuevoPlan_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_AlertasParametros_NuevoPlan_Obtener(int CodEmpresa, string operadora)
        {
            return _bl.Fnd_AlertasParametros_NuevoPlan_Obtener(CodEmpresa, operadora);
        }

        [Authorize]
        [HttpPost("Fnd_AlertasParametros_Registrar")]
        public ErrorDto Fnd_AlertasParametros_Registrar(int codEmpresa, string usuario, FndalertasData alerta)
        {
            return _bl.Fnd_AlertasParametros_Registrar(codEmpresa, usuario, alerta);
        }

        [Authorize]
        [HttpPost("Fnd_AlertasParametros_Alerta_Guardar")]
        public ErrorDto Fnd_AlertasParametros_Alerta_Guardar(int CodEmpresa, FndalertasData data)
        {
            return _bl.Fnd_AlertasParametros_Alerta_Guardar(CodEmpresa, data);
        }

        #endregion

        #region EMAIL

        [Authorize]
        [HttpPost("Fnd_AlertasEmail_Guardar")]
        public ErrorDto Fnd_AlertasEmail_Guardar(int codEmpresa, FndAlertasContactosDto contacto)
        {
            return _bl.Fnd_AlertasEmail_Guardar(codEmpresa, contacto);
        }

        [Authorize]
        [HttpPost("Fnd_AlertasEmail_Eliminar")]
        public ErrorDto Fnd_AlertasEmail_Eliminar(int codEmpresa, string usuario, List<FndAlertasContactosDto> listaContactos)
        {
            return _bl.Fnd_AlertasEmail_Eliminar(codEmpresa, usuario, listaContactos);
        }

        [Authorize]
        [HttpDelete("Fnd_AlertasEmailId_Eliminar")]
        public ErrorDto Fnd_AlertasEmailId_Eliminar(int codEmpresa, string usuario, int idregistro)
        {
            return _bl.Fnd_AlertasEmailId_Eliminar(codEmpresa, usuario, idregistro);
        }

        #endregion
    }
}