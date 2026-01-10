using Galileo.DataBaseTier;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;


namespace Galileo.BusinessLogic
{
    public class FrmCprCompraDevBL
    {
        private readonly FrmCprCompraDevDB _db;

        public FrmCprCompraDevBL(IConfiguration config)
        {
            _db = new FrmCprCompraDevDB(config);
        }

        public ErrorDto<List<FacturasDto>> ObtenerListaFacturas(int CodEmpresa, int CodProveedor)
        {
            return _db.ObtenerListaFacturas(CodEmpresa, CodProveedor);
        }

        public ErrorDto<FacturaDto?> ObtenerFactura(int CodEmpresa, string CodFactura, int CodProveedor)
        {
            return _db.ObtenerFactura(CodEmpresa, CodFactura, CodProveedor);
        }

        public ErrorDto<List<FacturaDetalleDto>> ObtenerFacturaDetalle(int CodEmpresa, string CodFactura, int CodProveedor)
        {
            return _db.ObtenerFacturaDetalle(CodEmpresa, CodFactura, CodProveedor);
        }
        public ErrorDto<List<BodegaDto>> ObtenerBodegas(int CodEmpresa)
        {
            return _db.ObtenerBodegas(CodEmpresa);
        }
        public ErrorDto VerificaFactura(int CodEmpresa, string CodFactura, int CodProveedor)
        {
            return _db.VerificaFactura(CodEmpresa, CodFactura, CodProveedor);
        }
        public ErrorDto<DevolucionData?> Devolucion_Obtener(int CodEmpresa, string CodDevolucion)
        {
            return _db.Devolucion_Obtener(CodEmpresa, CodDevolucion);
        }

        public ErrorDto<List<FacturaDetalleDto>> ObtenerDevolucionDetalle(int CodEmpresa, string CodDevolucion)
        {
            return _db.ObtenerDevolucionDetalle(CodEmpresa, CodDevolucion);
        }

        public ErrorDto<FacturaDto?> ObtenerOrdenCompraDev(int CodEmpresa, string CodFactura, int CodProveedor)
        {
            return _db.ObtenerOrdenCompraDev(CodEmpresa, CodFactura, CodProveedor);
        }

        public ErrorDto Devolucion_Guardar(int CodEmpresa, DevolucionInsert orden)
        {
            return _db.Devolucion_Guardar(CodEmpresa, orden);
        }
    }
}
