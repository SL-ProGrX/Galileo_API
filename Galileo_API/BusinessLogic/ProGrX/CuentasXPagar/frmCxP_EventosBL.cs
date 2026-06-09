using Galileo.DataBaseTier;
using Galileo.Models.CxP;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic.ProGrX.CxP
{
    public class FrmCxPEventosBL
    {
        private readonly FrmCxPEventosDB _db;

        public FrmCxPEventosBL(IConfiguration config)
        {
            _db = new FrmCxPEventosDB(config);
        }

        public ErrorDto<CxPEventos> Eventos_Obtener(int CodCliente, string cod_evento)
        {
            return _db.Eventos_Obtener(CodCliente, cod_evento);
        }

        public ErrorDto top1EventoObtener(int CodCliente, int Scroll, string cod_evento)
        {
            return _db.top1EventoObtener(CodCliente, Scroll, cod_evento);
        }

        public ErrorDto Evento_Guardar(int CodCliente, CxPEventos evento)
        {
            return _db.Evento_Guardar(CodCliente, evento);
        }

        public ErrorDto Evento_Eliminar(int CodCliente, string cod_evento)
        {
            return _db.Evento_Eliminar(CodCliente, cod_evento);
        }

        public ErrorDto<List<CxPEventosProveedor>> ObtenerProveedoresEvento(int CodEmpresa, string cod_evento)
        {
            return _db.ObtenerProveedoresEvento(CodEmpresa, cod_evento);
        }

        public ErrorDto AsignaEventoProveedor(int CodCliente, int proveedor, string evento, int activa, string usuario)
        {
            return _db.AsignaEventoProveedor(CodCliente, proveedor, evento, activa, usuario);
        }

        public ErrorDto<List<CxPEventosBusqueda>> EventosLista_Obtener(int CodEmpresa)
        {
            return _db.EventosLista_Obtener(CodEmpresa);
        }

        public ErrorDto<List<CxPEventosLineas>> EventosLineas_Obtener(int CodEmpresa, string cod_evento)
        {
            return _db.EventosLineas_Obtener(CodEmpresa, cod_evento);
        }
    }
}