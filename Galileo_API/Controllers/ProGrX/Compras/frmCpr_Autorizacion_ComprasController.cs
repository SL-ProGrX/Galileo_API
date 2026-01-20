using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;
using Microsoft.AspNetCore.Authorization;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class FrmCprAutorizacionComprasController : ControllerBase
    {
        private readonly FrmCprAutorizacionComprasBL _bl;
        public FrmCprAutorizacionComprasController(IConfiguration config)
        {
            _bl = new FrmCprAutorizacionComprasBL(config);
        }

        [HttpGet("SolicitudAutorizacion_Obtener")]
        public ErrorDto<List<CprSolicitudAutoriza>> SolicitudAutorizacion_Obtener(int CodCliente, string filtros)
        {
            return _bl.SolicitudAutorizacion_Obtener(CodCliente, filtros);
        }

        [HttpPost("AutorizaSolicitudes")]
        public ErrorDto AutorizaSolicitudes(int CodCliente, string solicitudes, string usuario)
        {
            return _bl.AutorizaSolicitudes(CodCliente, solicitudes, usuario);
        }

        [HttpPost("RechazaSolicitudes")]
        public ErrorDto RechazaSolicitudes(int CodCliente, string solicitudes, string justificacion, string usuario)
        {
            return _bl.RechazaSolicitudes(CodCliente, solicitudes, justificacion, usuario);
        }
    }
}