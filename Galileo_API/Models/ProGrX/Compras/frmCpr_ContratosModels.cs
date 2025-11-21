namespace Galileo.Models.CPR
{
    public class CprContratosFiltros
    {
        public int? pagina { get; set; }
        public int? paginacion { get; set; }
        public string? filtro { get; set; }
    }

    public class CprContratosLista
    {
        public int total { get; set; }
        public List<CprContratosDto>? contratos { get; set; } = new List<CprContratosDto>();
    }

    public class CprContratosDto
    {
        public string cod_contrato { get; set; } = string.Empty;
        public string? descripcion { get; set; } = string.Empty;
        public int cod_proveedor { get; set; }
        public string? proveedor { get; set; } = string.Empty;
        public string? estado { get; set; } = string.Empty;
        public int? periodo_garantia { get; set; }
        public string? tipo_contrato { get; set; }
        public int? plazo { get; set; }
        public string? cantidad_plazo { get; set; } = string.Empty;
        public float? monto { get; set; }
        public string? cta_contable { get; set; }
        public string? notas { get; set; } = string.Empty;
        public Nullable<DateTime> fecha_inicio { get; set; }
        public DateTime? fecha_corte { get; set; }
        public Nullable<DateTime> registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime? modifica_fecha { get; set; }
        public string? modifica_usuario { get; set; } = string.Empty;
        public string? divisa { get; set; } = string.Empty;
        public string? cod_centro_costo { get; set; } = string.Empty;
        public Nullable<DateTime> fecha_notificacion { get; set; }
        public Nullable<DateTime> fecha_vencimiento { get; set; }
        public string? fiscal { get; set; } = string.Empty;
        public int? porcentaje_garantia { get; set; }
        public float? monto_garantia { get; set; }
        public string? divisa_garantia { get; set; } = string.Empty;
    }

    public class CprContratosAdendumsDto
    {
        public int id_addendum { get; set; }
        public string cod_contrato { get; set; } = string.Empty;
        public string cod_contrato_madre { get; set; } = string.Empty;
        public string? notas { get; set; } = string.Empty;
        public Nullable<DateTime> registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
    }

    public class CprContratosEstadosDto
    {
        public int linea_id { get; set; }
        public string cod_contrato { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public Nullable<DateTime> fecha_inicio { get; set; }
        public string? notas { get; set; } = string.Empty;
        public Nullable<DateTime> registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
    }

    public class CprContratosProductosDto
    {
        public int linea_id { get; set; }
        public string cod_contrato { get; set; } = string.Empty;
        public string cod_producto { get; set; } = string.Empty;
        public string? descripcion { get; set; } = string.Empty;
        public Nullable<DateTime> registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
    }

    public class CprContratosProrrogasDto
    {
        public int id_prorroga { get; set; }
        public string cod_contrato { get; set; } = string.Empty;
        public Nullable<DateTime> fecha { get; set; }
        public string? motivos { get; set; } = string.Empty;
        public Nullable<DateTime> registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
    }

    public class CprContratosBitacoraDto
    {
        public int id_bitacora { get; set; }
        public string cod_contrato { get; set; } = string.Empty;
        public string movimiento { get; set; } = string.Empty;
        public string detalle { get; set; } = string.Empty;
        public Nullable<DateTime> registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
    }
}