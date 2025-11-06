namespace PgxAPI.Models
{
    public class CompraInventarioDto
    {
        public string CodProducto { get; set; } = string.Empty;
        public decimal? Cantidad { get; set; }
        public string CodBodega { get; set; } = string.Empty;
        public string CodTipo { get; set; } = string.Empty;
        public string Origen { get; set; } = string.Empty;
        public string Fecha { get; set; } = string.Empty;
        public decimal? Precio { get; set; }
        public decimal? ImpConsumo { get; set; }
        public decimal? ImpVentas { get; set; }
        public string TipoMov { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
    }
    
    public class BodegaDto
    {
        public string permite_entradas { get; set; } = string.Empty;
        public string permite_salidas { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
    }

    public class InvInventarioUpdate
    {
        public string vCodProd { get; set; } = string.Empty;
        public int vCantidad { get; set; }
        public string vBodega { get; set; } = string.Empty;
        public string vCodTipo { get; set; } = string.Empty;
        public string vOrigen { get; set; } = string.Empty;
        public string vFecha { get; set; } = string.Empty;
        public float vPrecio { get; set; }
        public float vImpCon { get; set; }
        public float vImpVenta { get; set; }
        public string vTipo { get; set; } = string.Empty;
    }

    public class ParametroValor
    {
        public string Cod_Parametro { get; set; } = string.Empty;
        public string Valor { get; set; } = string.Empty;
    }

    public class ConsultaDescripcion
    {
        public string CodX { get; set; } = string.Empty;
        public string DescX { get; set; } = string.Empty;

    }

    public class BitacoraProductoInsertarDto
    {
        public int EmpresaId { get; set; }
        public int id_bitacora { get; set; }
        public string cod_producto { get; set; } = string.Empty;
        public long? consec { get; set; }
        public string movimiento { get; set; } = string.Empty;
        public string detalle { get; set; } = string.Empty;
        public string registro_usuario { get; set; } = string.Empty;
    }

    public class BitacoraProveedorInsertarDto
    {
        public int EmpresaId { get; set; }
        public int id_bitacora { get; set; }
        public string cod_proveedor { get; set; } = string.Empty;
        public long? consec { get; set; }
        public string movimiento { get; set; } = string.Empty;
        public string detalle { get; set; } = string.Empty;
        public string registro_usuario { get; set; } = string.Empty;
    }

    public class BitacoraComprasInsertarDto
    {
        public int EmpresaId { get; set; }
        public int id_bitacora { get; set; }
        public string id_compra { get; set; } = string.Empty;
        public long? consec { get; set; }
        public string movimiento { get; set; } = string.Empty;
        public string detalle { get; set; } = string.Empty;
        public string registro_usuario { get; set; } = string.Empty;
    }
    
    public class FndControlAutorizaData
    {
        public int CodEmpresa { get; set; } = 0;
        public int tipoCambio { get; set; } = 0;
        public string usuario { get; set; } = string.Empty;
        public string strSQL { get; set; } = string.Empty;
    }

    public class FndControlCambioAprobDto
    {
        public int id_cambio { get; set; }
        public int cod_tipo_cambio { get; set; }
        public string? nom_tabla { get; set; }
        public string? llaves { get; set; }
        public string? cod_evento { get; set; }
        public string? valoresjsonact { get; set; }
        public string? valoresjsondif { get; set; }
        public string? cod_estado { get; set; }
        public string? usuario_cambio { get; set; }
        public Nullable<DateTime> fecha_cambio { get; set; }
        public string? usuario_modifica { get; set; }
        public Nullable<DateTime> fecha_modifica { get; set; }
        public string? usuario_aprueba { get; set; }
        public Nullable<DateTime> fecha_aprueba { get; set; }
        public string? observaciones { get; set; }
    }

    public class CampoCambio
    {
        public string? Campo { get; set; }
        public object? ValorNuevo { get; set; }
    }

    public class DropDownListaGenericaModel
    {
        public object? item { get; set; }
        public string descripcion { get; set; } = string.Empty;
    }

    public class DropDownListaGenericaModel<T>
    {
        public required T item { get; set; }
        public string descripcion { get; set; } = string.Empty;
    }

    public class TablasListaGenericaModel
    {
        public int total { get; set; }
        public object? lista { get; set; }
    }

    public class FiltrosLazyLoadData
    {
        public string? filtro { get; set; } //filtro del buscar en tablas o buscador
        public int pagina { get; set; } = 1;//pagina de la tabla
        public int paginacion { get; set; } = 30; //paginacion de la tabla
        public object? parametros { get; set; } //adicional para enviar JSON con filtros adicionales
        public int sortOrder { get; set; } = 0; //0: sin orden, 1: ascendente, 2: descendente
        public string? sortField { get; set; } //campo por el cual se ordena
        public object? filters { get; set; } //filtros de encabezados
    }

    public class TesBancosArchivosData
    {
        public int utiliza_formato_especial { get; set; }
        public string archivo_cheques_firmas { get; set; } = string.Empty;
        public string archivo_cheques_sin_firmas { get; set; } = string.Empty;
    }

    public class TesArchivosEspecialesData
    {
        public string chequesFirmas { get; set; } = string.Empty;
        public string chequesSinFirmas { get; set; } = string.Empty;
    }

    public class TesDivisaAsiento
    {
        public string cod_divisa { get; set; } = string.Empty;
        public float tipo_cambio { get; set; } = 0;
    }
}