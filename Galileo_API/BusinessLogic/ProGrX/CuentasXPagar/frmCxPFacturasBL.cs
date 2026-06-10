using Galileo.DataBaseTier;
using Galileo.Models.CxP;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic.ProGrX.CxP
{
    public class FrmCxPFacturasBL
    {
        private readonly FrmCxPFacturasDB _db;

        public FrmCxPFacturasBL(IConfiguration config)
        {
            _db = new FrmCxPFacturasDB(config);
        }

        public ErrorDto<List<ParametrosIva>> ParamIVA_Obtener(int CodEmpresa)
        {
            return _db.ParamIVA_Obtener(CodEmpresa);
        }

        public ErrorDto<DivisaLocal> DivisaLocal_Obtener(int CodEmpresa)
        {
            return _db.DivisaLocal_Obtener(CodEmpresa);
        }

        public ErrorDto<List<Divisa>> Divisas_Obtener(int CodEmpresa)
        {
            return _db.Divisas_Obtener(CodEmpresa);
        }

        public ErrorDto<List<Unidad>> Unidades_Obtener(int CodEmpresa)
        {
            return _db.Unidades_Obtener(CodEmpresa);
        }

        public ErrorDto<List<CentroCosto>> CentrosCosto_Obtener(int CodEmpresa, string Cod_Unidad)
        {
            return _db.CentrosCosto_Obtener(CodEmpresa, Cod_Unidad);
        }

        public ErrorDto<FacturaLista> Facturas_Obtener(int CodEmpresa, int Cod_Proveedor, int? pagina, int? paginacion, string? filtro)
        {
            return _db.Facturas_Obtener(CodEmpresa, Cod_Proveedor, pagina, paginacion, filtro);
        }

        public ErrorDto<FacturaDto> FacturaDetalle_Obtener(int CodEmpresa, string Cod_Factura, int Cod_Proveedor)
        {
            return _db.FacturaDetalle_Obtener(CodEmpresa, Cod_Factura, Cod_Proveedor);
        }

        public ErrorDto<List<AsientoFactura>> FacturaAsientos_Obtener(int CodEmpresa, string Cod_Factura, int Cod_Proveedor)
        {
            return _db.FacturaAsientos_Obtener(CodEmpresa, Cod_Factura, Cod_Proveedor);
        }
        public ErrorDto FacturaNumero_Cambiar(int CodEmpresa, FacturaCambioNo data)
        {
            return _db.FacturaNumero_Cambiar(CodEmpresa, data);
        }

        public ErrorDto FacturaImpuesto_Actualizar(int CodEmpresa, FacturaImpuesto data)
        {
            return _db.FacturaImpuesto_Actualizar(CodEmpresa, data);
        }

        public ErrorDto<ProveedorFactura> ProveedorFactura_Obtener(int CodEmpresa, int Cod_Proveedor)
        {
            return _db.ProveedorFactura_Obtener(CodEmpresa, Cod_Proveedor);
        }

        public ErrorDto<FacturaAntSig> ConsultaAscDesc(int CodEmpresa, string Cod_Factura, string tipo)
        {
            return _db.ConsultaAscDesc(CodEmpresa, Cod_Factura, tipo);
        }

        public ErrorDto Factura_Anular(int CodEmpresa, FacturaAnular data)
        {
            return _db.Factura_Anular(CodEmpresa, data);
        }

        public ErrorDto<FacturaPlantillaLista> Plantillas_Obtener(int CodEmpresa, int? pagina, int? paginacion, string? filtro)
        {
            return _db.Plantillas_Obtener(CodEmpresa, pagina, paginacion, filtro);
        }

        public ErrorDto<List<AsientoFactura>> PlantillaAsientos_Obtener(int CodEmpresa, int Cod_Plantilla, string fecha, decimal total)
        {
            return _db.PlantillaAsientos_Obtener(CodEmpresa, Cod_Plantilla, fecha, total);
        }

        public ErrorDto<List<Factura>> PlantillaFactura_Obtener(int CodEmpresa)
        {
            return _db.PlantillaFactura_Obtener(CodEmpresa);
        }

        public ErrorDto<CuentaProveedor> CuentaProveedor_Obtener(int CodEmpresa, int Cod_Proveedor)
        {
            return _db.CuentaProveedor_Obtener(CodEmpresa, Cod_Proveedor);
        }

        public int TipoCambio_Obtener(int CodEmpresa, string cod_Divisa, string Fecha)
        {
            return _db.TipoCambio_Obtener(CodEmpresa, cod_Divisa, Fecha);
        }


        public ErrorDto FacturaAsientos_Borrar(int CodEmpresa, string Cod_Factura, int Cod_Proveedor)
        {
            return _db.FacturaAsientos_Borrar(CodEmpresa, Cod_Factura, Cod_Proveedor);
        }

        public ErrorDto SaldoPagarProv_Actualizar(int CodEmpresa, decimal Saldo, decimal Saldo_Divisa, int Cod_Proveedor)
        {
            return _db.SaldoPagarProv_Actualizar(CodEmpresa, Saldo, Saldo_Divisa, Cod_Proveedor);
        }

        public ErrorDto FacturaAsiento_Insertar(int CodEmpresa, AsientoFactura data)
        {
            return _db.FacturaAsiento_Insertar(CodEmpresa, data);
        }

        public ErrorDto PagoContado_Insertar(int CodEmpresa, PagoContado data)
        {
            return _db.PagoContado_Insertar(CodEmpresa, data);
        }

        public ErrorDto Factura_Insertar(int CodEmpresa, FacturaDto data)
        {
            return _db.Factura_Insertar(CodEmpresa, data);
        }
    }
}