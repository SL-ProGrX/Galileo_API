namespace Galileo.Models.CPR
{
    public class CompraDto
    {
        public string Datakey { get; set; } = string.Empty;
        public string Cod_Factura { get; set; } = string.Empty;
        public string Proveedor { get; set; } = string.Empty;
        public string Cod_Orden { get; set; } = string.Empty;
        public string Cod_Compra { get; set; } = string.Empty;
    }

    public class CompraAnulacionDto
    {
        public string Cod_Factura { get; set; } = string.Empty;
        public int Cod_Proveedor { get; set; }
        public string Cod_Orden { get; set; } = string.Empty;
        public string Cod_Compra { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Forma_Pago { get; set; } = string.Empty;
        public string Cxp_Estado { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public decimal Sub_Total { get; set; }
        public string Notas { get; set; } = string.Empty;
        public decimal Descuento { get; set; }
        public decimal Imp_Ventas { get; set; }
        public decimal Imp_Consumo { get; set; }
        public decimal Total { get; set; }
        public string Asiento_Estado { get; set; } = string.Empty;
        public DateTime Asiento_Fecha { get; set; }
    }

    public class CompraDetalleDto
    {
        public short Linea { get; set; }
        public string Cod_Factura { get; set; } = string.Empty;
        public int Cod_Proveedor { get; set; }
        public string Cod_Producto { get; set; } = string.Empty;
        public string Producto { get; set; } = string.Empty;
        public string Cod_Bodega { get; set; } = string.Empty;
        public string Bodega { get; set; } = string.Empty;
        public decimal? Cantidad { get; set; }
        public decimal? Precio { get; set; }
        public decimal? Cantidad_Devuelta { get; set; }
        public decimal? Imp_Ventas { get; set; }
        public decimal? Imp_Consumo { get; set; }
        public decimal? Descuento { get; set; }
        public decimal? Total { get; set; }
    }

    public class CompraAnulacionDatosDto
    {
        public string Cod_Factura { get; set; } = string.Empty;
        public int Cod_Proveedor { get; set; }
        public string Cod_Orden { get; set; } = string.Empty;
        public string Cod_Compra { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Forma_Pago { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public decimal Sub_Total { get; set; }
        public string Notas { get; set; } = string.Empty;
        public decimal Descuento { get; set; }
        public decimal Imp_Ventas { get; set; }
        public decimal Imp_Consumo { get; set; }
        public decimal Total { get; set; }
        public string Cxp_Estado { get; set; } = string.Empty;
        public string Asiento_Estado { get; set; } = string.Empty;
        public DateTime Asiento_Fecha { get; set; }
        public string Causa { get; set; } = string.Empty;
        public string Proveedor { get; set; } = string.Empty;
        public string Nota { get; set; } = string.Empty;
        public DateTime Anula_Fecha { get; set; }
        public DateTime Anula_Fec_Afecta { get; set; }
        public string Genera_User { get; set; } = string.Empty;
    }

    public class CompraAnulacionDatosRequestDto
    {
        public string codigoCompra { get; set; } = string.Empty;
        public string codigoOrden { get; set; } = string.Empty;
        public string codigoProveedor { get; set; } = string.Empty;
    }

    public class CargosDto
    {
        public string Cod_Cargo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Cod_Cuenta { get; set; } = string.Empty;
        public int Activo { get; set; }
    }
}