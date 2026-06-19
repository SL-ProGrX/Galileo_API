using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.INV;

namespace Galileo.BusinessLogic
{
    public class FrmInvCambioPreciosBL
    {
        private readonly FrmInvCambioPreciosDB _db;

        public FrmInvCambioPreciosBL(IConfiguration config)
        {
            _db = new FrmInvCambioPreciosDB(config);
        }

        public ErrorDto<List<FacturaPrecioDetalleDto>> OrdenesDetalle_Obtener(int CodEmpresa, string CodFactura, int? CodProveedor)
        {
            return _db.OrdenesDetalle_Obtener(CodEmpresa, CodFactura, CodProveedor);
        }

        public ErrorDto PreciosFactura_Actualiza(int CodEmpresa, FacturaPrecioDetalleDto data)
        {
            return _db.PreciosFactura_Actualiza(CodEmpresa, data);
        }

        public ErrorDto CambiosPrecios_Actualizar(int CodEmpresa, PrecioExcelDto precio)
        {
            return _db.CambiosPrecios_Actualizar(CodEmpresa, precio);
        }
    }
}