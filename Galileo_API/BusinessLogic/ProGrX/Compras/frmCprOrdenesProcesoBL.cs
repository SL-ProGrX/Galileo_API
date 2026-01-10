using Galileo.DataBaseTier;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic
{
    public class FrmCprOrdenesProcesoBL
    {
        private readonly FrmCprOrdenesProcesoDB _db;

        public FrmCprOrdenesProcesoBL(IConfiguration config)
        {
            _db = new FrmCprOrdenesProcesoDB(config);
        }

        public ErrorDto<List<ProveedorOrdenesData>> ProveedorOrden_Obtener(int CodEmpresa, string CodOrden)
        {
            return _db.ProveedorOrden_Obtener(CodEmpresa, CodOrden);
        }

        public ErrorDto Cpr_Orden_Proceso(int CodEmpresa, OrdenProceso orden)
        {
            return _db.Cpr_Orden_Proceso(CodEmpresa, orden);
        }

        public ErrorDto OrdenProceso_ReemplazarPin(int CodEmpresa, bool pinIngreso, string pin, string CodOrden)
        {
            return _db.OrdenProceso_ReemplazarPin(CodEmpresa, pinIngreso, pin, CodOrden);
        }

        public ErrorDto Orden_Autoriza(int CodEmpresa, string CodOrden, string usuario, int index)
        {
            return _db.Orden_Autoriza(CodEmpresa, CodOrden, usuario, index);
        }

        public ErrorDto Orden_Rechaza(int CodEmpresa, string CodOrden, string usuario, int index)
        {
            return _db.Orden_Autoriza(CodEmpresa, CodOrden, usuario, index);
        }

        public ErrorDto Orden_Cerrar(int CodEmpresa, string CodOrden)
        {
            return _db.Orden_Cerrar(CodEmpresa, CodOrden);
        }

        public ErrorDto ProveedorEstado_Obtener(int CodEmpresa, int CodProveedor)
        {
            return _db.ProveedorEstado_Obtener(CodEmpresa, CodProveedor);
        }
    }
}
