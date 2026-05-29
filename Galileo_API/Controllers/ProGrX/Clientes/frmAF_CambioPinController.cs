using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Clientes;
using Galileo_API.Models.ProGrX.Clientes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Clientes
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmAfCambioPinController : ControllerBase
    {
        private readonly FrmAfCambioPinBL _bl;

        public FrmAfCambioPinController(IConfiguration config)
        {
            _bl = new FrmAfCambioPinBL(config);
        }

        [HttpGet("fxgAFIParametro")]
        public ErrorDto<string> fxgAFIParametro(int CodEmpresa, string pCodigo)
        { 
            return _bl.fxgAFIParametro(CodEmpresa, pCodigo);
        }

        [HttpGet("Af_CambioPin_ObtenerPersona")]
        public ErrorDto<FrmAfCambioPinPersonaModel> Af_CambioPin_ObtenerPersona(int CodEmpresa, string cedula)
        {
            return _bl.Af_CambioPin_ObtenerPersona(CodEmpresa, cedula);
        }

        [HttpGet("fxTicketValida")]
        public ErrorDto fxTicketValida(int CodEmpresa, string ticket)
        {
            return _bl.fxTicketValida(CodEmpresa, ticket);
        }

        [HttpGet("GenerarPinSeguro")]
        public string GenerarPinSeguro(int CodEmpresa)
        {
            var result = FrmAfCambioPinBL.GenerarPinSeguro(CodEmpresa);
            return result;
        }

        [HttpPost("Af_CambioPin_Bitacora")]
        public ErrorDto Af_CambioPin_Bitacora(int CodEmpresa, string usuario, string vTicket)
        {
            return _bl.Af_CambioPin_Bitacora(CodEmpresa, usuario, vTicket);
        }

        [HttpPost("Af_CambioPin_RenovarClaveWeb")]
        public ErrorDto Af_CambioPin_RenovarClaveWeb(
          int CodEmpresa,
          string cedula,
          string email,
          string usuario)
        {
            return _bl.Af_CambioPin_RenovarClaveWeb(CodEmpresa, cedula, email, usuario);
        }

        [HttpPost("Af_CambioPin_AplicarCambioPin")]
        public ErrorDto Af_CambioPin_AplicarCambioPin(
           int CodEmpresa,
           FrmAfCambioPinAplicarModel model)
        {
            return _bl.Af_CambioPin_AplicarCambioPin(CodEmpresa, model);
        }
    }
}