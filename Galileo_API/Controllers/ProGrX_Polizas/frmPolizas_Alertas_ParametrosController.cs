using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Polizas;
using Galileo_API.Models.ProGrX_Polizas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Polizas
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmPolizasAlertasParametrosController : ControllerBase
    {
        private readonly FrmPolizasAlertasParametrosBL _bl;

        public FrmPolizasAlertasParametrosController(IConfiguration config)
        {
            _bl = new FrmPolizasAlertasParametrosBL(config);
        }

        [HttpGet("POL_Alertas_Parametros_Obtener")]
        public ErrorDto<PolAlertasParametrosDto?> POL_Alertas_Parametros_Obtener(int CodEmpresa)
        {
            return _bl.POL_Alertas_Parametros_Obtener(CodEmpresa);
        }

        [HttpPost("POL_Alertas_Parametros_Guardar")]
        public ErrorDto POL_Alertas_Parametros_Guardar(int CodEmpresa, string Usuario, PolAlertasParametrosGuardarDto param)
        {
            return _bl.POL_Alertas_Parametros_Guardar(CodEmpresa, Usuario, param);
        }

        [HttpGet("POL_Alertas_Email_Listar")]
        public ErrorDto<List<PolAlertasEmailDto>> POL_Alertas_Email_Listar(int CodEmpresa)
        {
            return _bl.POL_Alertas_Email_Listar(CodEmpresa);
        }

        [HttpPost("POL_Alertas_Email_Agregar")]
        public ErrorDto POL_Alertas_Email_Agregar(int CodEmpresa, string Usuario, PolAlertasEmailAgregarDto dto)
        {
            return _bl.POL_Alertas_Email_Agregar(CodEmpresa, Usuario, dto);
        }

        [HttpDelete("POL_Alertas_Email_Eliminar")]
        public ErrorDto POL_Alertas_Email_Eliminar(int CodEmpresa, string Usuario, int ids)
        {
            return _bl.POL_Alertas_Email_Eliminar(CodEmpresa, Usuario, ids);
        }

    }
}
