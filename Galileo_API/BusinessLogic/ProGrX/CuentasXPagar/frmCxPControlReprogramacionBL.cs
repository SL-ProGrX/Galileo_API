using Galileo.DataBaseTier;
using Galileo.Models.CxP;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic.ProGrX.CxP
{
    public class FrmCxPControlReprogramacionBL
    {
        private readonly FrmCxPControlReprogramacionDB _db;

        public FrmCxPControlReprogramacionBL(IConfiguration config)
        {
            _db = new FrmCxPControlReprogramacionDB(config);
        }

        public ErrorDto<FacturaLista> Facturas_Obtener(int CodEmpresa, int Cod_Proveedor, int? pagina, int? paginacion, string? filtro)
        {
            return _db.Facturas_Obtener(CodEmpresa, Cod_Proveedor, pagina, paginacion, filtro);
        }

        public ErrorDto<VCxpProgramacionPago> ProgramacionDetalle_Obtener(int CodEmpresa, string Cod_Factura, int Cod_Proveedor)
        {
            return _db.ProgramacionDetalle_Obtener(CodEmpresa, Cod_Factura, Cod_Proveedor);
        }

        public ErrorDto<Pago> PagoMontos_Obtener(int CodEmpresa, string Cod_Factura, int Cod_Proveedor)
        {
            return _db.PagoMontos_Obtener(CodEmpresa, Cod_Factura, Cod_Proveedor);
        }

        public ErrorDto FacturaMonto_Ajuste(int CodEmpresa, AjusteFactura data)
        {
            return _db.FacturaMonto_Ajuste(CodEmpresa, data);
        }

        public ErrorDto<FacturaDet> FacturaDetalle_Obtener(int CodEmpresa, string Cod_Factura, int Cod_Proveedor)
        {
            return _db.FacturaDetalle_Obtener(CodEmpresa, Cod_Factura, Cod_Proveedor);
        }

        public ErrorDto<List<CargoAdicional>> CargosAdicionales_Obtener(int CodEmpresa, string Cod_Factura, int Cod_Proveedor)
        {
            return _db.CargosAdicionales_Obtener(CodEmpresa, Cod_Factura, Cod_Proveedor);
        }

        public ErrorDto<FacturaDatos> CompraDatos_Obtener(int CodEmpresa, string Cod_Factura, int Cod_Proveedor)
        {
            return _db.CompraDatos_Obtener(CodEmpresa, Cod_Factura, Cod_Proveedor);
        }

        public ErrorDto<FacturaDatos> FacturaDatos_Obtener(int CodEmpresa, string Cod_Factura, int Cod_Proveedor)
        {
            return _db.FacturaDatos_Obtener(CodEmpresa, Cod_Factura, Cod_Proveedor);
        }

        public ErrorDto CargosPagos_Borrar(int CodEmpresa, int Pago, string Cod_Factura, int Cod_Proveedor)
        {
            return _db.CargosPagos_Borrar(CodEmpresa, Pago, Cod_Factura, Cod_Proveedor);
        }

        public ErrorDto Reprogramacion_Aplicar(int CodEmpresa, ReprogramacionAplicar data)
        {
            return _db.Reprogramacion_Aplicar(CodEmpresa, data);
        }
    }
}
