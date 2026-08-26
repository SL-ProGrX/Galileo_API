using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.CuentasxCobrar;
using Galileo_API.Models.ProGrX.CuentasxCobrar;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc; 

namespace Galileo_API.Controllers.ProGrX.CuentasxCobrar
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCxCConsultaController : ControllerBase
    {
        private readonly FrmCxCConsultaBL _bl;

        public FrmCxCConsultaController(IConfiguration config)
            => _bl = new FrmCxCConsultaBL(config);

        [Authorize]
        [HttpGet("ConsultarPersona")]
        public ErrorDto<CxCPersonaDto?> ConsultarPersona(
            int codEmpresa,
            string cedula)
        {
            return _bl.ConsultarPersona(codEmpresa, cedula);
        }

        [Authorize]
        [HttpGet("ConsultarPersonasF4")]
        public ErrorDto<CxCPersonasF4ListaDto> ConsultarPersonasF4(
            int codEmpresa,
            [FromQuery] CxCPersonasF4FiltroDto request)
        {
            return _bl.ConsultarPersonasF4(codEmpresa, request);
        }

        [Authorize]
        [HttpGet("ConsultarCuentas")]
        public ErrorDto<List<CxCCuentaDto>> ConsultarCuentas(int codEmpresa,string cedula,string estado)
        {
            return _bl.ConsultarCuentas(codEmpresa, cedula, estado);
        }

        [Authorize]
        [HttpGet("ConsultarSolicitudes")]
        public ErrorDto<List<CxCSolicitudDto>> ConsultarSolicitudes(int codEmpresa,string cedula)
        {
            return _bl.ConsultarSolicitudes(codEmpresa, cedula);
        }

        [Authorize]
        [HttpGet("ConsultarPreAnalisis")]
        public ErrorDto<List<CxCPreAnalisisDto>> ConsultarPreAnalisis(int codEmpresa,string cedula)
        {
            return _bl.ConsultarPreAnalisis(codEmpresa, cedula);
        }

        [Authorize]
        [HttpGet("ConsultarIncobrables")]
        public ErrorDto<List<CxCIncobrableDto>> ConsultarIncobrables(int codEmpresa,string cedula)
        {
            return _bl.ConsultarIncobrables(codEmpresa, cedula);
        }

        [Authorize]
        [HttpPost("ConsultarFacturas")]
        public ErrorDto<List<CxCFacturaDto>> ConsultarFacturas(int codEmpresa,CxCFacturaFiltroDto filtro)
        {
            return _bl.ConsultarFacturas(codEmpresa, filtro);
        }

        [Authorize]
        [HttpGet("ConsultarDesembolsos")]
        public ErrorDto<List<CxCDesembolsoDto>> ConsultarDesembolsos(int codEmpresa,string cedula)
        {
            return _bl.ConsultarDesembolsos(codEmpresa, cedula);
        }

        [Authorize]
        [HttpGet("ConsultarMensajes")]
        public ErrorDto<List<CxCMensajeDto>> ConsultarMensajes(int codEmpresa,string cedula)
        {
            return _bl.ConsultarMensajes(codEmpresa, cedula);
        }

        [Authorize]
        [HttpPost("GuardarMensaje")]
        public ErrorDto<bool> GuardarMensaje(int codEmpresa,CxCMensajeAddDto dto)
        {
            return _bl.GuardarMensaje(codEmpresa, dto);
        }

        [Authorize]
        [HttpDelete("EliminarMensaje")]
        public ErrorDto<bool> EliminarMensaje(int codEmpresa,CxCMensajeDeleteDto dto)
        {
            return _bl.EliminarMensaje(codEmpresa, dto);
        }

        [Authorize]
        [HttpGet("ConsultarFacturasPorGiro")]
        public ErrorDto<List<CxCDesembolsoFacturaDto>> ConsultarFacturasPorGiro(int codEmpresa,int operacion,int idGiro)
        {
            return _bl.ConsultarFacturasPorGiro(codEmpresa, operacion, idGiro);
        }

        [Authorize]
        [HttpGet("ConsultarEstadosFactura")]
        public ErrorDto<List<CxCFacturaEstadoDto>> ConsultarEstadosFactura(int codEmpresa)
        {
            return _bl.ConsultarEstadosFactura(codEmpresa);
        }
    }

}
