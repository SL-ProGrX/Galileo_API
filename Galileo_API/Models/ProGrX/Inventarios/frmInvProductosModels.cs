namespace PgxAPI.Models.INV
{
    public class ProductoDto
    {
        public string codigo { get; set; } = string.Empty;
        public string Cod_Producto { get; set; } = string.Empty;
        public string Cod_Marca { get; set; } = string.Empty;
        public string Cod_Unidad { get; set; } = string.Empty;
        public int Cod_Prodclas { get; set; }
        public string Cod_Barras { get; set; } = string.Empty;
        public int Lotes { get; set; }
        public bool Lotesbool { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public string Tipo_Producto { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string Observacion { get; set; } = string.Empty;
        public decimal Costo_Regular { get; set; }
        public decimal Precio_Regular { get; set; }
        public string Dir_Fotografia { get; set; } = string.Empty;
        public string Cod_Fabricante { get; set; } = string.Empty;
        public decimal Comision_Monto { get; set; }
        public decimal Comision_Unidad { get; set; }
        public decimal Impuesto_Ventas { get; set; }
        public decimal Impuesto_Consumo { get; set; }
        public string Inventario_Calcula { get; set; } = string.Empty;
        public bool Inventario_Calculabool { get; set; }
        public decimal Inventario_Minimo { get; set; }
        public decimal Inventario_Maximo { get; set; }
        public decimal Fracciones { get; set; }
        public decimal Precio_Compra { get; set; }
        public string Descuento_Tipo { get; set; } = string.Empty;
        public decimal Descuento_Valor { get; set; }
        public string Cod_Cuenta { get; set; } = string.Empty;
        public decimal Existencia { get; set; }
        public string User_Crea { get; set; } = string.Empty;
        public string User_Modifica { get; set; } = string.Empty;
        public Nullable<DateTime> Ultima_Modificacion { get; set; }
        public decimal Porc_Utilidad { get; set; }
        public decimal Tipo_Cambio { get; set; }
        public int Similar { get; set; }
        public string Cod_Linea_Sub { get; set; } = string.Empty;
        public string Cabys { get; set; } = string.Empty;
        public Nullable<DateTime> Fe_Sinc_Fecha { get; set; }
        public string Fe_Sinc_User { get; set; } = string.Empty;
        public string ProdClass { get; set; } = string.Empty;
        public string UnidadDesc { get; set; } = string.Empty;
        public string MarcaDesc { get; set; } = string.Empty;
        public string? LineaSub { get; set; }
        public string? LineaSubCod { get; set; }
        public string? tipo_activo { get; set; }
        public bool i_filtrado { get; set; } = false;
        public float punto_reorden { get; set; } = 0;
        public int tiempo_entrega_dias { get; set; } = 0;
        public string Presentacion { get; set; } = string.Empty;
        public int Cant_Presentacion { get; set; } = 0;
        public int Volumen { get; set; } = 0;
        public string justificacion_estado { get; set; } = string.Empty;
    }

    public class Producto
    {
        public string Cod_Producto { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }

    public class PrecioProducto
    {
        public string Cod_Producto { get; set; } = string.Empty;
        public string Cod_Precio { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public decimal Utilidad { get; set; }
    }

    public class MovimientoProducto
    {
        public string Tipo { get; set; } = string.Empty;
        public string TipoDesc { get; set; } = string.Empty;
        public string Boleta { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string Procesa_User { get; set; } = string.Empty;
        public DateTime Procesa_Fecha { get; set; }
        public string BodegaO { get; set; } = string.Empty;
        public string BodegaD { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public decimal Cantidad { get; set; }
    }

    public class BonificacionProducto
    {
        public string Cod_Producto { get; set; } = string.Empty;
        public int Consec { get; set; }
        public int Desde { get; set; }
        public int Hasta { get; set; }
        public int Bonificacion { get; set; }
    }

    public class DescuentoProducto
    {
        public string Cod_Producto { get; set; } = string.Empty;
        public int Consec { get; set; }
        public decimal Desde { get; set; }
        public decimal Hasta { get; set; }
        public decimal Porcentaje { get; set; }
    }

    public class SimilarProducto
    {
        public string Cod_Producto { get; set; } = string.Empty;
        public string Cod_Producto_Similar { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Cabys { get; set; } = string.Empty;
    }

    public class ProveedorProducto
    {
        public int Cod_Proveedor { get; set; }
        public string Cod_Producto { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Cedjur { get; set; } = string.Empty;
        public DateTime Fecha_Factura { get; set; }
        public int CodX { get; set; }
    }

    public class ProvProductoDataLista
    {
        public int Total { get; set; }
        public List<ProveedorProducto> Lista { get; set; } = new List<ProveedorProducto>();
    }

    public class BodegaExistenciaProducto
    {
        public string Cod_Producto { get; set; } = string.Empty;
        public string Cod_Bodega { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal Existencias { get; set; }
        public DateTime Fecha_Corte { get; set; }
        public string Usuario { get; set; } = string.Empty;
    }

    public class CabysHereda
    {
        public string Cabys { get; set; } = string.Empty;
    }

    public class UensProductos
    {
        public string cod_unidad { get; set; } = string.Empty;
        public string? cod_producto { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public string cntx_unidad { get; set; } = string.Empty;
        public string registro_usuario { get; set; } = string.Empty;
        public bool asignado { get; set; } = false;
    }

    public class TipoActivoList
    {
        public string item { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class BitacoraProductosDto
    {
        public int id_bitacora { get; set; }
        public string cod_producto { get; set; } = string.Empty;
        public int consec { get; set; }
        public string movimiento { get; set; } = string.Empty;
        public string detalle { get; set; } = string.Empty;
        public DateTime registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;


    }
}