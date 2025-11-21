namespace Galileo.Models
{
    public class ProveedoresDataLista
    {
        public int Total { get; set; }
        public List<ProveedorData> Proveedores { get; set; } = new List<ProveedorData>();
    }

    public class ProveedorData
    {
        public string Cod_Proveedor { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Cedjur { get; set; } = string.Empty;
    }

    public class CargoDataLista
    {
        public int Total { get; set; }
        public List<CargoData> Cargos { get; set; } = new List<CargoData>();
    }

    public class CargoData
    {
        public int Cod_Cargo { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public float Monto { get; set; }
    }

    public class BodegaDataLista
    {
        public int Total { get; set; }
        public List<BodegaData> bodegas { get; set; } = new List<BodegaData>();
    }

    public class BodegaData
    {
        public string cod_bodega { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class ArticuloDataLista
    {
        public int Total { get; set; }
        public List<ArticuloData> Articulos { get; set; } = new List<ArticuloData>();
    }

    public class ArticuloData
    {
        public string codigo { get; set; } = string.Empty;

        public string Cod_Producto { get; set; } = string.Empty;
        public string Cabys { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Cod_Barras { get; set; } = string.Empty;
        public decimal Existencia { get; set; }
        public decimal Costo_Regular { get; set; }
        public decimal Precio_Regular { get; set; }
        public decimal Impuesto_Ventas { get; set; }
        public string Cod_Fabricante { get; set; } = string.Empty;
        public bool I_Stock { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string unidad { get; set; } = string.Empty;
    }

    public class ArticuloDataFiltros
    {
        public int? catalogo { get; set; }
        public string? filtro { get; set; }
        public int? pagina { get; set; }
        public int? paginacion { get; set; }
        public string? cod_unidad { get; set; }
        public int? familia { get; set; }
        public string? sublinea { get; set; }

    }

    public class ArticuloUenDatos
    {
        public string? cod_producto { get; set; }
        public string? cod_unidad { get; set; }

    }

    public class OrdenesDataLista
    {
        public int Total { get; set; }
        public List<OrdenData> Ordenes { get; set; } = new List<OrdenData>();
    }

    public class OrdenData
    {
        public string Cod_Orden { get; set; } = string.Empty;
        public string Genera_User { get; set; } = string.Empty;
        public string? Nota { get; set; }
        public string? cod_solicitud { get; set; }
        public string? proveedor { get; set; }
        public string? familia { get; set; }
    }

    public class FacturasData
    {
        public string cod_factura { get; set; } = string.Empty;
        public string Proveedor { get; set; } = string.Empty;
        public decimal Total { get; set; }
    }

    public class FacturasDataLista
    {
        public int Total { get; set; }
        public List<FacturasData> Facturas { get; set; } = new List<FacturasData>();
    }

    public class UsuarioDataLista
    {
        public int Total { get; set; }
        public List<UsuarioData> Usuarios { get; set; } = new List<UsuarioData>();
    }

    public class UsuarioData
    {
        public string nombre { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class FacturasProveedorLista
    {
        public int Total { get; set; }
        public List<FacturasProveedorData> Facturas { get; set; } = new List<FacturasProveedorData>();
    }

    public class FacturasProveedorDataFiltros
    {
        public int? cod_proveedor { get; set; }
        public string? filtro { get; set; }
        public int pagina { get; set; } = 0;
        public int paginacion { get; set; } = 30;
    }

    public class FacturasProveedorData
    {
        public string cod_compra { get; set; } = string.Empty;
        public string cod_orden { get; set; } = string.Empty;
        public string cod_factura { get; set; } = string.Empty;
        public string no_solicitud { get; set; } = string.Empty;
        public string proveedor { get; set; } = string.Empty;
        public int? cod_proveedor { get; set; }
    }

    public class CompraDevLista
    {
        public int Total { get; set; }
        public List<CompraDevData> devoluciones { get; set; } = new List<CompraDevData>();
    }

    public class CompraDevData
    {
        public string cod_compra_dev { get; set; } = string.Empty;
        public string Proveedor { get; set; } = string.Empty;
        public string cod_factura { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public DateTime fecha { get; set; }
    }

    public class BeneficioDataLista
    {
        public int Total { get; set; }
        public List<BeneficioData> Beneficios { get; set; } = new List<BeneficioData>();
    }

    public class BeneficioData
    {
        public string cod_beneficio { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class SociosDataLista
    {
        public int Total { get; set; }
        public List<SociosData> socios { get; set; } = new List<SociosData>();
    }

    public class SociosData
    {
        public string cedula { get; set; } = string.Empty;
        public string cedular { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string membresia { get; set; } = string.Empty;
    }

    public class BeneficioProductoLista
    {
        public int Total { get; set; }
        public List<BeneficioProductoData> productos { get; set; } = new List<BeneficioProductoData>();
    }

    public class BeneficioProductoData
    {
        public string cod_producto { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public float costo_unidad { get; set; }
    }

    public class DepartamentoData
    {
        public string cod_departamento { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class DepartamentoDataLista
    {
        public int Total { get; set; }
        public List<DepartamentoData> departamentos { get; set; } = new List<DepartamentoData>();
    }

    public class CatalogosLista
    {
        public string item { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class ProveedorDataFiltros
    {
        public string? estado { get; set; }
        public bool? autoGestion { get; set; }
        public bool? ventas { get; set; }
        public string? filtro { get; set; }
        public int? pagina { get; set; }
        public int? paginacion { get; set; }
    }
}
