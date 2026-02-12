using Galileo_API.BusinessLogic.ProGrX.CuentasxCobrar;
using Galileo_API.Models.ProGrX.CuentasxCobrar;
using Galileo.Models.ERROR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Galileo_API.Controllers.ProGrX.CuentasxCobrar
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCxCContratosSuscripcionesController : ControllerBase
    {
        private readonly FrmCxCContratosSuscripcionesBL _bl;

        public FrmCxCContratosSuscripcionesController(IConfiguration config)
        {
            _bl = new FrmCxCContratosSuscripcionesBL(config);
        }

        [Authorize]
        [HttpGet("CxcPersonas_Lista")]
        public ErrorDto<List<CxcPersonaDto>> CxcPersonas_Lista(int codEmpresa)
        {
            return _bl.CxcPersonas_Lista(codEmpresa);
        }

        [Authorize]
        [HttpGet("CxcPersonaContrato_Obtener")]
        public ErrorDto<CxcPersonaContratoDto?> CxcPersonaContrato_Obtener(int codEmpresa, [FromQuery] string cedula, [FromQuery] string codContrato)
        {
            return _bl.CxcPersonaContrato_Obtener(codEmpresa, cedula, codContrato);
        }

        [Authorize]
        [HttpPost("CxcPersonaContrato_Guardar")]
        public ErrorDto<bool> CxcPersonaContrato_Guardar(int codEmpresa, [FromBody] CxcPersonaContratoSaveParams param)
        {
            return _bl.CxcPersonaContrato_Guardar(codEmpresa, param);
        }

        [Authorize]
        [HttpPost("CxcPersonaContrato_Eliminar")]
        public ErrorDto<bool> CxcPersonaContrato_Eliminar(int codEmpresa, [FromBody] CxcPersonaContratoDeleteParams param)
        {
            return _bl.CxcPersonaContrato_Eliminar(codEmpresa, param);
        }

        [Authorize]
        [HttpGet("CxcPersonaContratoPagadores_Lista")]
        public ErrorDto<List<CxcPersonaContratoPagadorDto>> CxcPersonaContratoPagadores_Lista(int codEmpresa, [FromQuery] string codContrato, [FromQuery] string cedula)
        {
            return _bl.CxcPersonaContratoPagadores_Lista(codEmpresa, codContrato, cedula);
        }

        [Authorize]
        [HttpGet("CxcContratoPagadoresDisponibles_Lista")]
        public ErrorDto<List<CxcPersonaContratoPagadorDto>> CxcContratoPagadoresDisponibles_Lista(int codEmpresa, [FromQuery] string codContrato, [FromQuery] string cedula)
        {
            return _bl.CxcContratoPagadoresDisponibles_Lista(codEmpresa, codContrato, cedula);
        }

        [Authorize]
        [HttpPost("CxcPersonaContratoPagador_Insertar")]
        public ErrorDto<bool> CxcPersonaContratoPagador_Insertar(int codEmpresa, [FromBody] CxcPersonaContratoPagadorSaveParams param)
        {
            return _bl.CxcPersonaContratoPagador_Insertar(codEmpresa, param);
        }

        [Authorize]
        [HttpPost("CxcPersonaContratoPagador_Eliminar")]
        public ErrorDto<bool> CxcPersonaContratoPagador_Eliminar(int codEmpresa, [FromBody] CxcPersonaContratoPagadorDeleteParams param)
        {
            return _bl.CxcPersonaContratoPagador_Eliminar(codEmpresa, param);
        }

        [Authorize]
        [HttpGet("CxcPersonaContratoSuscripciones_Lista")]
        public ErrorDto<List<CxcPersonaContratoSuscripcionDto>> CxcPersonaContratoSuscripciones_Lista(int codEmpresa, [FromQuery] string codContrato, [FromQuery] string cedula)
        {
            return _bl.CxcPersonaContratoSuscripciones_Lista(codEmpresa, codContrato, cedula);
        }

        [Authorize]
        [HttpGet("CxcContratoCargosDisponibles_Lista")]
        public ErrorDto<List<CxcPersonaContratoSuscripcionDto>> CxcContratoCargosDisponibles_Lista(int codEmpresa, [FromQuery] string codContrato, [FromQuery] string cedula)
        {
            return _bl.CxcContratoCargosDisponibles_Lista(codEmpresa, codContrato, cedula);
        }

        [Authorize]
        [HttpPost("CxcPersonaContratoSuscripcion_Insertar")]
        public ErrorDto<bool> CxcPersonaContratoSuscripcion_Insertar(int codEmpresa, [FromBody] CxcPersonaContratoSuscripcionSaveParams param)
        {
            return _bl.CxcPersonaContratoSuscripcion_Insertar(codEmpresa, param);
        }

        [Authorize]
        [HttpPost("CxcPersonaContratoSuscripcion_Eliminar")]
        public ErrorDto<bool> CxcPersonaContratoSuscripcion_Eliminar(int codEmpresa, [FromBody] CxcPersonaContratoSuscripcionDeleteParams param)
        {
            return _bl.CxcPersonaContratoSuscripcion_Eliminar(codEmpresa, param);
        }
    }
}
