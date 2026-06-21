namespace Galileo.Models.CPR
{
    public class OrdenesData
    {
        public string cod_orden { get; set; } = string.Empty;
        public string genera_user { get; set; } = string.Empty;
        public string? nota { get; set; }
    }

    public class OrdenDto
    {
        public string cod_orden { get; set; } = string.Empty;
        public string tipo_orden { get; set; } = string.Empty;
        public string pin_entrada { get; set; } = string.Empty;
        public string pin_autorizacion { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string proceso { get; set; } = string.Empty;
        public string nota { get; set; } = string.Empty;
        public DateTime genera_fecha { get; set; }
        public string genera_user { get; set; } = string.Empty;
        public DateTime autoriza_fecha { get; set; }
        public string autoriza_user { get; set; } = string.Empty;
        public float subtotal { get; set; }
        public float descuento { get; set; }
        public float imp_ventas { get; set; }
        public float imp_consumo { get; set; }
        public float total { get; set; }
        public string cod_proveedor { get; set; } = string.Empty;
        public string plantilla { get; set; } = string.Empty;
        public string causa_id { get; set; } = string.Empty;
        public string causa_desc { get; set; } = string.Empty;
        public string proveedor_desc { get; set; } = string.Empty;
        public string? cedula_proveedor { get; set; } = string.Empty;
        public string? direccion_proveedor { get; set; } = string.Empty;
        public string? telefono_proveedor { get; set; } = string.Empty;
        public string? plazo_entrega { get; set; } = string.Empty;
        public string? garantia { get; set; } = string.Empty;
        public string? plazo_pago { get; set; } = string.Empty;
        public string? direccion_entrega { get; set; } = string.Empty;
        public string? horario_recepcion { get; set; } = string.Empty;
        public string? terminos_condiciones { get; set; } = string.Empty;
        public string? multa { get; set; } = string.Empty;
        public string? cod_solicitud { get; set; } = string.Empty;
        public string? cod_unidad { get; set; } = string.Empty;
        public string? divisa { get; set; } = string.Empty;
    }

    public class OrdenLineasData
    {
        public int total { get; set; }
        public long cantidad { get; set; }
        public List<OrdenLineas> lineas { get; set; } = new List<OrdenLineas>();
    }

    public class OrdenLineas
    {
        public string cod_producto { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public float cantidad { get; set; }
        public float precio { get; set; }
        public float descuento { get; set; }
        public float imp_ventas { get; set; }
        public float total { get; set; }
        public bool i_existe { get; set; }
        public bool i_completo { get; set; }
    }

    public class OrdenDataInsert
    {
        public string cod_Orden { get; set; } = string.Empty;
        public string Tipo_Orden { get; set; } = string.Empty;
        public string? nota { get; set; }
        public float descuento { get; set; }
        public string? genera_Fecha { get; set; }
        public float imp_ventas { get; set; }
        public string genera_User { get; set; } = string.Empty;
        public float subtotal { get; set; }
        public float total { get; set; }
    }

    public class OrderLineaTablaFiltros
    {
        public int? pagina { get; set; }
        public int? paginacion { get; set; }
        public string? filtro { get; set; }
        public string CodOrden { get; set; } = string.Empty;
    }

    public class OrdenDatosAcciones
    {
        public required bool edita { get; set; }
        public string cod_orden { get; set; } = string.Empty;
        public string nota { get; set; } = string.Empty;
        public string tipo_orden { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public string? plazo_entrega { get; set; } = string.Empty;
        public string? garantia { get; set; } = string.Empty;
        public string? plazo_pago { get; set; } = string.Empty;
        public string? direccion_entrega { get; set; } = string.Empty;
        public string? horario_recepcion { get; set; } = string.Empty;
        public string? terminos_condiciones { get; set; } = string.Empty;
        public string? multa { get; set; } = string.Empty;
        public List<OrdenLineas> lineas { get; set; } = new List<OrdenLineas>();
    }

    public class OrdenesUensData
    {
        public string cod_orden { get; set; } = string.Empty;
        public string cod_producto { get; set; } = string.Empty;
        public string cod_unidad { get; set; } = string.Empty;
        public required int cantidad { get; set; }
        public string tipo_producto { get; set; } = string.Empty;
        public string? registro_usuario { get; set; } = string.Empty;
        public Nullable<DateTime> registro_fecha { get; set; }
    }

    public class CprHorarioLista
    {
        public string item { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class CprFormaPago
    {
        public string item { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }
}