using Galileo.DataBaseTier;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic
{
    public class FrmCprOrdenesBL
    {
        private readonly FrmCprOrdenesDB _db;

        public FrmCprOrdenesBL(IConfiguration config)
        {
            _db = new FrmCprOrdenesDB(config);
        }

        public ErrorDto<OrdenDto> OrdenSeleccionadaObtener(int CodEmpresa, string CodOrden, string usuario)
        {
            return _db.OrdenesSeleccionada(CodEmpresa, CodOrden, usuario);
        }

        public ErrorDto<OrdenLineasData> OrdenLineasObtener(int CodEmpresa, string filtros)
        {
            return _db.OrdenLineasObtener(CodEmpresa, filtros);
        }

        public ErrorDto<OrdenesData> Orden_scroll(int CodEmpresa, int scrollValue, string? cod_Orden)
        {
            return _db.Orden_scroll(CodEmpresa, scrollValue, cod_Orden);
        }

        public ErrorDto Orden_Insertar(int CodEmpresa, object jOrdenes)
        {
            return _db.Orden_Insertar(CodEmpresa, jOrdenes);
        }

        public ErrorDto Orden_Actualiza(int CodEmpresa, OrdenDatosAcciones jOrdenes)
        {
            return _db.Orden_Actualiza(CodEmpresa, jOrdenes);
        }

        public ErrorDto<List<OrdenesUensData>> OrdenesUENs_Obtener(int CodEmpresa, string CodOrden, string CodProducto)
        {
            return _db.OrdenesUENs_Obtener(CodEmpresa, CodOrden, CodProducto);
        }

        public ErrorDto OrdenesUENs_Guardar(int CodEmpresa, List<OrdenesUensData> lista)
        {
            return _db.OrdenesUENs_Guardar(CodEmpresa, lista);
        }

        public ErrorDto OrdenesUENs_Eliminar(int CodEmpresa, string cod_orden, string cod_producto, string cod_unidad)
        {
            return _db.OrdenesUENs_Eliminar(CodEmpresa, cod_orden, cod_producto, cod_unidad);
        }

        public ErrorDto<List<CprHorarioLista>> horarios_Obtener(int CodEmpresa, string usuario)
        {
            return _db.horarios_Obtener(CodEmpresa, usuario);
        }

        public ErrorDto<List<CprFormaPago>> formapago_Obtener(int CodEmpresa, string usuario)
        {
            return _db.formapago_Obtener(CodEmpresa, usuario);
        }

        public ErrorDto CorreoNotificaOrdenCompra(int CodEmpresa, string cod_orden, string proveedor, string cod_proveedor)
        {
            return _db.CorreoNotificaOrdenCompra(CodEmpresa, cod_orden, proveedor,cod_proveedor);
        }

    }
}
