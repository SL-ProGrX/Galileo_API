using Microsoft.AspNetCore.Mvc;
using Galileo.Models.CxP;
using Galileo.Models.ERROR;
using Microsoft.AspNetCore.Authorization;
using Galileo.BusinessLogic.ProGrX.CxP;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class FrmCxPEventosController : ControllerBase
    {
        private readonly FrmCxPEventosBL _bl;
        public FrmCxPEventosController(IConfiguration config)
        {
            _bl = new FrmCxPEventosBL(config);
        }

        [HttpGet("Eventos_Obtener")]
        public ErrorDto<CxPEventos> Eventos_Obtener(int CodCliente, string cod_evento)
        {
            return _bl.Eventos_Obtener(CodCliente, cod_evento);
        }

        [HttpGet("top1EventoObtener")]
        public ErrorDto top1EventoObtener(int CodCliente, int Scroll, string cod_evento)
        {
            return _bl.top1EventoObtener(CodCliente, Scroll, cod_evento);
        }

        [HttpPost("Evento_Guardar")]
        public ErrorDto Evento_Guardar(int CodCliente, CxPEventos evento)
        {
            return _bl.Evento_Guardar(CodCliente, evento);
        }

        [HttpDelete("Evento_Eliminar")]
        public ErrorDto Evento_Eliminar(int CodCliente, string cod_evento)
        {
            return _bl.Evento_Eliminar(CodCliente, cod_evento);
        }

        [HttpGet("ObtenerProveedoresEvento")]
        public ErrorDto<List<CxPEventosProveedor>> ObtenerProveedoresEvento(int CodEmpresa, string? cod_evento)
        {
            return _bl.ObtenerProveedoresEvento(CodEmpresa, cod_evento ?? string.Empty);
        }

        [HttpPost("AsignaEventoProveedor")]
        public ErrorDto AsignaEventoProveedor(int CodCliente, int proveedor, string evento, int activa, string usuario)
        {
            return _bl.AsignaEventoProveedor(CodCliente, proveedor, evento, activa, usuario);
        }

        [HttpGet("EventosLista_Obtener")]
        public ErrorDto<List<CxPEventosBusqueda>> EventosLista_Obtener(int CodEmpresa)
        {
            return _bl.EventosLista_Obtener(CodEmpresa);
        }

        [HttpGet("EventosLineas_Obtener")]
        public ErrorDto<List<CxPEventosLineas>> EventosLineas_Obtener(int CodEmpresa, string cod_evento)
        {
            return _bl.EventosLineas_Obtener(CodEmpresa, cod_evento);
        }
    }
}