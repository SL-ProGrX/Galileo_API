namespace PgxAPI.Models.CPR
{
    public class CprFacturasXMLFiltros
    {
        public int? pagina { get; set; }
        public int? paginacion { get; set; }
        public string? filtro { get; set; }
    }

    public class CprFacturasXMLLista
    {
        public int total { get; set; }
        public List<CprFacturasXML_DTO>? lista { get; set; } = new List<CprFacturasXML_DTO>();
    }

    public class CprFacturasXML_DTO
    {
        public int id { get; set; }
        public string cod_uen { get; set; } = string.Empty;
        public string cod_documento { get; set; } = string.Empty;
        public string clave { get; set; } = string.Empty;
        public string ced_jur_prov { get; set; } = string.Empty;
        public string nombre_prov { get; set; } = string.Empty;
        public decimal monto_total { get; set; } 
        public string cod_divisa { get; set; } = string.Empty;
        public DateTime fecha { get; set; }
        public string estado { get; set; } = string.Empty;
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime registro_fecha { get; set; }

        public string cod_proveedor { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;

        public List<CprFacturasLineasXML_Data>? lineas { get; set; }
    }

    public class CprFacturasLineasXML_Data
    {
        public int? numeroLinea { get; set; }
        public string? codigo { get; set; }
        public string? codigoComercial { get; set; }
        public decimal? cantidad { get; set; }
        public string? unidadMedida { get; set; }
        public string? unidadMedidaComercial { get; set; }
        public string? detalle { get; set; }
        public decimal? precioUnitario { get; set; }
        public decimal? montoTotal { get; set; }
        public decimal? subTotal { get; set; }
        public decimal? impuesto { get; set; }
        public decimal? montoTotalLinea { get; set; }
        public bool? inv_existe { get; set; }
    }

    public class  CprValidaProducto
    {
        public string? COD_PRODUCTO { get; set; }
        public string? DESCRIPCION { get; set; }
    }

}
