using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using Microsoft.AspNetCore.Authorization;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class FrmInvOrdenesAutorizacionController : ControllerBase
    {
        private readonly FrmInvOrdenesAutorizacionBL _bl;

        public FrmInvOrdenesAutorizacionController(IConfiguration config)
        {
            _bl = new FrmInvOrdenesAutorizacionBL(config);
        }

        [HttpGet("resolucionTransaccion_Obtener")]
        public ErrorDto<List<ResolucionTransaccionDto>> resolucionTransaccion_Obtener(int CodCliente, string filtros)
        {
            return _bl.resolucionTransaccion_Obtener(CodCliente, filtros);
        }

        [HttpPost("ResolucionTransaccion_Autorizar")]
        public ErrorDto ResolucionTransaccion_Autorizar(int CodCliente, string tipo, string usuario, List<ResolucionTransaccionDto> lista)
        {
            return _bl.ResolucionTransaccion_Autorizar(CodCliente, tipo, usuario, lista);
        }

        [HttpPost("ResolucionTransaccion_Rechazo")]
        public ErrorDto ResolucionTransaccion_Rechazo(int CodCliente, string tipo, string usuario, List<ResolucionTransaccionDto> lista)
        {
            return _bl.ResolucionTransaccion_Rechazo(CodCliente, tipo, usuario, lista);
        }
    }
}