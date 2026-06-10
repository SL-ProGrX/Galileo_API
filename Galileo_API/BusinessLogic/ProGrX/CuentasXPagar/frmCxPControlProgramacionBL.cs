using Galileo.DataBaseTier;
using Galileo.Models.CxP;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic.ProGrX.CxP
{
    public class FrmCxPControlProgramacionBL
    {
        private readonly FrmCxPControlProgramacionDB _db;

        public FrmCxPControlProgramacionBL(IConfiguration config)
        {
            _db = new FrmCxPControlProgramacionDB(config);
        }

        public ErrorDto<ProgramacionPagoLista> PagosFactura_Obtener(int CodEmpresa, int? pagina, int? paginacion, string? filtro, ConsultaPagosParam param)
        {
            return _db.PagosFactura_Obtener(CodEmpresa, pagina, paginacion, filtro, param);
        }

        public ErrorDto<List<CargoAdicional>> CargosAdicionales_Obtener(int CodEmpresa)
        {
            return _db.CargosAdicionales_Obtener(CodEmpresa);
        }

        public ErrorDto<SaldosInformacion> DetalleSaldos_Obtener(int CodEmpresa, string Cod_Factura, int Cod_Proveedor)
        {
            return _db.DetalleSaldos_Obtener(CodEmpresa, Cod_Factura, Cod_Proveedor);
        }

        public ErrorDto<FacturaDatos> CompraDatos_Obtener(int CodEmpresa, string Cod_Factura, int Cod_Proveedor)
        {
            return _db.CompraDatos_Obtener(CodEmpresa, Cod_Factura, Cod_Proveedor);
        }

        public ErrorDto<FacturaDatos> FacturaDatos_Obtener(int CodEmpresa, string Cod_Factura, int Cod_Proveedor)
        {
            return _db.FacturaDatos_Obtener(CodEmpresa, Cod_Factura, Cod_Proveedor);
        }

        public ErrorDto<List<DetallePago>> DetallePagos_Obtener(int CodEmpresa, string Cod_Factura, int Cod_Proveedor)
        {
            return _db.DetallePagos_Obtener(CodEmpresa, Cod_Factura, Cod_Proveedor);
        }

        public ErrorDto<TesoreriaDetalle> TesoreriaDetalle_Obtener(int CodEmpresa, int Tesoreria)
        {
            return _db.TesoreriaDetalle_Obtener(CodEmpresa, Tesoreria);
        }

        public ErrorDto SaldosProveedor_Actualizar(int CodEmpresa, decimal Saldo, decimal Tipo_Cambio, int Cod_Proveedor)
        {
            return _db.SaldosProveedor_Actualizar(CodEmpresa, Saldo, Tipo_Cambio, Cod_Proveedor);
        }

        public ErrorDto FacturaEstado_Actualizar(int CodEmpresa, string Cod_Factura, int Cod_Proveedor)
        {
            return _db.FacturaEstado_Actualizar(CodEmpresa, Cod_Factura, Cod_Proveedor);
        }

        public ErrorDto CompraEstado_Actualizar(int CodEmpresa, string Cod_Factura, int Cod_Proveedor)
        {
            return _db.CompraEstado_Actualizar(CodEmpresa, Cod_Factura, Cod_Proveedor);
        }

        public ErrorDto Pago_Insertar(int CodEmpresa, DetallePago data)
        {
            return _db.Pago_Insertar(CodEmpresa, data);
        }

        public ErrorDto PagoProvCargo_Insertar(int CodEmpresa, PagoProvCargo data)
        {
            return _db.PagoProvCargo_Insertar(CodEmpresa, data);
        }

        public ErrorDto<Disponible> Disponible_Obtener(int CodEmpresa, int NPago, string Cod_Factura, int Cod_Proveedor)
        {
            return _db.Disponible_Obtener(CodEmpresa, NPago, Cod_Factura, Cod_Proveedor);
        }

        public ErrorDto PagoProv_Actualizar(int CodEmpresa, string Usuario, string Cod_Factura, int Cod_Proveedor)
        {
            return _db.PagoProv_Actualizar(CodEmpresa, Usuario, Cod_Factura, Cod_Proveedor);
        }

        public ErrorDto FechaVencimiento_Actualizar(int CodEmpresa, DetallePago data)
        {
            return _db.FechaVencimiento_Actualizar(CodEmpresa, data);
        }
    }
}