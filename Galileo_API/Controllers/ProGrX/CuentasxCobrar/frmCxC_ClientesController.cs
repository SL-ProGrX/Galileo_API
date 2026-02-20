using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.CuentasxCobrar;
using Galileo_API.Models.ProGrX.CuentasxCobrar;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Galileo_API.Controllers.ProGrX.CuentasxCobrar
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCxCClientesController : ControllerBase
    {
        private readonly FrmCxCClientesBL _bl;

        public FrmCxCClientesController(IConfiguration config)
        {
            _bl = new FrmCxCClientesBL(config);
        }

        [Authorize]
        [HttpGet("CxcPersonas_Lista")]
        public ErrorDto<List<CxcPersonaDto>> CxcPersonas_Lista(int codEmpresa, string orden)
        {
            return _bl.CxcPersonas_Lista(codEmpresa, orden);
        }

        [Authorize]
        [HttpGet("EstadoCivil_Lista")]
        public ErrorDto<List<DropDownListaGenericaModel>> EstadoCivil_Lista(int codEmpresa)
        {
            return _bl.EstadoCivil_Lista(codEmpresa);
        }

        [Authorize]
        [HttpGet("Clasificacion_Lista")]
        public ErrorDto<List<DropDownListaGenericaModel>> Clasificacion_Lista(int codEmpresa)
        {
            return _bl.Clasificacion_Lista(codEmpresa);
        }

        [Authorize]
        [HttpGet("TiposId_Lista")]
        public ErrorDto<List<DropDownListaGenericaModel>> TiposId_Lista(int codEmpresa)
        {
            return _bl.TiposId_Lista(codEmpresa);
        }

        [Authorize]
        [HttpGet("Provincias_Lista")]
        public ErrorDto<List<DropDownListaGenericaModel>> Provincias_Lista(int codEmpresa)
        {
            return _bl.Provincias_Lista(codEmpresa);
        }

        [Authorize]
        [HttpGet("Cantones_Lista")]
        public ErrorDto<List<DropDownListaGenericaModel>> Cantones_Lista(int codEmpresa, [FromQuery] string provincia)
        {
            return _bl.Cantones_Lista(codEmpresa, provincia);
        }

        [Authorize]
        [HttpGet("Distritos_Lista")]
        public ErrorDto<List<DropDownListaGenericaModel>> Distritos_Lista(int codEmpresa, [FromQuery] string provincia, [FromQuery] string canton)
        {
            return _bl.Distritos_Lista(codEmpresa, provincia, canton);
        }

        [Authorize]
        [HttpGet("CxcPersona_Valida")]
        public ErrorDto<CxcPersonaValidaResult?> CxcPersona_Valida(int codEmpresa, string cedula)
        {
            return _bl.CxcPersona_Valida(codEmpresa, cedula);
        }

        [Authorize]
        [HttpGet("Socio_Info")]
        public ErrorDto<SocioInfoDto?> Socio_Info(int codEmpresa, string cedula)
        {
            return _bl.Socio_Info(codEmpresa, cedula);
        }

        [Authorize]
        [HttpGet("Persona_Info")]
        public ErrorDto<PersonaInfoDto?> Persona_Info(int codEmpresa, string cedula)
        {
            return _bl.Persona_Info(codEmpresa, cedula);
        }

        [Authorize]
        [HttpGet("CxcPersona_LargoCedula")]
        public ErrorDto<CxcPersonaLargoCedulaResult?> CxcPersona_LargoCedula(int codEmpresa, short tipoId)
        {
            return _bl.CxcPersona_LargoCedula(codEmpresa, tipoId);
        }

        [Authorize]
        [HttpPost("CxcPersona_Guardar")]
        public ErrorDto<bool> CxcPersona_Guardar(int codEmpresa, [FromBody] CxcPersonaSaveParams param)
        {
            return _bl.CxcPersona_Guardar(codEmpresa, param);
        }

        [Authorize]
        [HttpPost("CxcPersona_Eliminar")]
        public ErrorDto<bool> CxcPersona_Eliminar(int codEmpresa, [FromBody] CxcPersonaDeleteParams param)
        {
            return _bl.CxcPersona_Eliminar(codEmpresa, param);
        }

        [Authorize]
        [HttpGet("CxcPersonasCuentas")]
        public ErrorDto<List<CxcPersonaCuentaDto>> CxcPersonasCuentas(int codEmpresa, string cedula, string estado)
        {
            return _bl.CxcPersonasCuentas(codEmpresa, cedula, estado);
        }

        [Authorize]
        [HttpGet("CxcPersonasContratos")]
        public ErrorDto<List<CxcPersonaContratosDto>> CxcPersonasContratos(int codEmpresa, string cedula)
        {
            return _bl.CxcPersonasContratos(codEmpresa, cedula);
        }

        [Authorize]
        [HttpGet("CxcPersonasContratosPagadores")]
        public ErrorDto<List<CxcPersonaContratosPagadorDto>> CxcPersonasContratosPagadores(int codEmpresa, string codContrato, string cedula)
        {
            return _bl.CxcPersonasContratosPagadores(codEmpresa, codContrato, cedula);
        }

        [Authorize]
        [HttpGet("CxcPersonasContratosSuscripciones")]
        public ErrorDto<List<CxcPersonaContratosSuscripcionDto>> CxcPersonasContratosSuscripciones(int codEmpresa, string codContrato, string cedula)
        {
            return _bl.CxcPersonasContratosSuscripciones(codEmpresa, codContrato, cedula);
        }

        [Authorize]
        [HttpPost("CxcContratoPagador_Eliminar")]
        public ErrorDto<bool> CxcContratoPagador_Eliminar(int codEmpresa, [FromBody] CxcContratosPagadorDeleteParams param)
        {
            return _bl.CxcContratoPagador_Eliminar(codEmpresa, param);
        }

        [Authorize]
        [HttpPost("CxcContratoSuscripcion_Eliminar")]
        public ErrorDto<bool> CxcContratoSuscripcion_Eliminar(int codEmpresa, [FromBody] CxcContratosSuscripcionDeleteParams param)
        {
            return _bl.CxcContratoSuscripcion_Eliminar(codEmpresa, param);
        }

        [Authorize]
        [HttpGet("CxcCuentasBancarias")]
        public ErrorDto<List<CxcCuentaBancariaDto>> CxcCuentasBancarias(int codEmpresa, string cedula)
        {
            return _bl.CxcCuentasBancarias(codEmpresa, cedula);
        }
    }
}
