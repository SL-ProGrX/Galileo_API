using Newtonsoft.Json;
using Galileo.DataBaseTier;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic
{
    public class FrmCprComprasOrdenBL
    {
        private readonly FrmCprComprasOrdenDB _db;

        public FrmCprComprasOrdenBL(IConfiguration config)
        {
            _db = new FrmCprComprasOrdenDB(config);
        }

        public ErrorDto<OrdenCompraSinFacturaData> Orden_Obtener(int CodEmpresa, string CodOrden)
        {
            return _db.Orden_Obtener(CodEmpresa, CodOrden);
        }

        public ErrorDto<OrdenCompraFacturaData> OrdenFactura_Obtener(int CodEmpresa, string CodOrden)
        {
            return _db.OrdenFactura_Obtener(CodEmpresa, CodOrden);
        }
        public ErrorDto<CompraOrdenLineasData> OrdenesDetalle_Obtener(int CodEmpresa, string Jfiltros)
        {
            CompraOrderLineaTablaFiltros filtros = JsonConvert.DeserializeObject<CompraOrderLineaTablaFiltros>(Jfiltros) ?? new CompraOrderLineaTablaFiltros();

             return filtros.CodProveedor != 0
                 ? _db.OrdenesDetalleF_Obtener(CodEmpresa, filtros)
                 : _db.OrdenesDetalleO_Obtener(CodEmpresa, filtros);

        }

        public ErrorDto ComprasOrden_Guardar(int CodEmpresa, ComprasOrdenDatos orden)
        {
            return _db.ComprasOrden_Guardar(CodEmpresa, orden);
        }

        public ErrorDto OrdenPin_Obtener(int CodEmpresa, string CodOrden)
        {
            return _db.OrdenPin_Obtener(CodEmpresa, CodOrden);
        }

        public ErrorDto OrdenPin_Verifica(int CodEmpresa, string CodOrden, string OrdPin)
        {
            return _db.OrdenPin_Verifica(CodEmpresa, CodOrden, OrdPin);
        }

        public ErrorDto OrdenConsecutivo_Obtener(int CodEmpresa)
        {
            return _db.OrdenConsecutivo_Obtener(CodEmpresa);
        }

        public ErrorDto<List<FacturasAutorizarDto>> FacturasAutorizar_Obtener(int CodEmpresa, string usuario, int proveedor)
        {
            return _db.FacturasAutorizar_Obtener(CodEmpresa, usuario, proveedor);
        }

        public ErrorDto Factura_AutorizarRechazar(int CodEmpresa,string usuario, string cod, string cod_factura,string justificacion)
        {
            return _db.Factura_AutorizarRechazar(CodEmpresa, usuario, cod, cod_factura, justificacion);
        }

        public ErrorDto ValidaAutorizacion(int CodEmpresa, string usuario, string cod_orden)
        {
            return _db.ValidaAutorizacion(CodEmpresa, usuario, cod_orden);
        }

    }
}
