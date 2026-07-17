namespace Galileo_API.Models.ProGrX_Pasivos
{
    public class FrmCrApaCortesGarantiasCatalogoDto
    {
        public string idx { get; set; } = string.Empty;
        public string itmx { get; set; } = string.Empty;
    }

    public class FrmCrApaCortesGarantiasEncabezadoDto
    {
        public string cod_acreedor { get; set; } = string.Empty;
        public string acreedor { get; set; } = string.Empty;
        public string operacion { get; set; } = string.Empty;
        public decimal saldo { get; set; }
    }

    public class FrmCrApaCortesGarantiasCorteDto
    {
        public DateTime fecha_corte { get; set; }
        public decimal saldo_operacion { get; set; }
        public decimal saldo_garantias { get; set; }
        public decimal responsabilidad { get; set; }
        public decimal diferencia { get; set; }
        public string estado { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
    }

    public class FrmCrApaCortesGarantiasCorteDatosDto
    {
        public string cod_acreedor { get; set; } = string.Empty;
        public string operacion { get; set; } = string.Empty;
        public DateTime fecha_corte { get; set; }
        public DateTime? registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime? cierre_fecha { get; set; }
        public string cierre_usuario { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string estado_desc { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public decimal saldo_operacion { get; set; }
        public decimal saldo_responsabilidad { get; set; }
    }

    public class FrmCrApaCortesGarantiasDetalleDto
    {
        public int id_solicitud { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public int operacion { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public int solicitud { get; set; }
        public decimal monto { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public decimal monto_solicitado { get; set; }
        public decimal cuota { get; set; }
        public decimal saldo { get; set; }
        public string categoria { get; set; } = string.Empty;
        public string linea { get; set; } = string.Empty;
        [System.Text.Json.Serialization.JsonIgnore]
        public string codigo { get; set; } = string.Empty;
        [System.Text.Json.Serialization.JsonIgnore]
        public string cod_linea { get; set; } = string.Empty;
        [System.Text.Json.Serialization.JsonIgnore]
        public string linea_credito { get; set; } = string.Empty;
        public DateTime? fecha_formalizacion { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public DateTime? fecha_formaliza { get; set; }
        public string garantia { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public decimal tasa { get; set; }
        public int plazo { get; set; }
        public int mora_cuotas { get; set; }
        public decimal mora_intereses { get; set; }
        public decimal mora_principal { get; set; }
        public DateTime? fecha_termina { get; set; }
        public string bucket { get; set; } = string.Empty;
        public string correo { get; set; } = string.Empty;
        public string telefono { get; set; } = string.Empty;
        public string provincia { get; set; } = string.Empty;
        public string canton { get; set; } = string.Empty;
        public string distrito { get; set; } = string.Empty;
    }

    public class FrmCrApaCortesGarantiasTotalesDto
    {
        public decimal saldo_operacion { get; set; }
        public decimal saldo_garantias { get; set; }
        public decimal responsabilidad { get; set; }
        public decimal diferencia { get; set; }
    }

    public class FrmCrApaCortesGarantiasFiltrosDto
    {
        public string estado { get; set; } = string.Empty;
        public string categoria { get; set; } = string.Empty;
        public string garantias { get; set; } = string.Empty;
        public DateTime fecha_desde { get; set; }
        public DateTime fecha_hasta { get; set; }
        public string destino { get; set; } = string.Empty;
        public string recurso { get; set; } = string.Empty;
        public string linea { get; set; } = string.Empty;
        public decimal saldo_mayor { get; set; }
        public decimal mora_mayor { get; set; }
        public string bucket { get; set; } = string.Empty;
    }

    public class FrmCrApaCortesGarantiasConsultaRequest
    {
        public string cod_acreedor { get; set; } = string.Empty;
        public string operacion { get; set; } = string.Empty;
        public DateTime fecha_corte { get; set; }
        public bool filtrar { get; set; }
        public FrmCrApaCortesGarantiasFiltrosDto filtros { get; set; } = new();
    }

    public class FrmCrApaCortesGarantiasGuardarRequest
    {
        public string cod_acreedor { get; set; } = string.Empty;
        public string operacion { get; set; } = string.Empty;
        public DateTime fecha_corte { get; set; }
        public string notas { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public bool editar { get; set; }
    }

    public class FrmCrApaCortesGarantiasClaveRequest
    {
        public string cod_acreedor { get; set; } = string.Empty;
        public string operacion { get; set; } = string.Empty;
        public DateTime fecha_corte { get; set; }
        public string usuario { get; set; } = string.Empty;
    }

    public class FrmCrApaCortesGarantiasExcluirRequest : FrmCrApaCortesGarantiasClaveRequest
    {
        public int tipo { get; set; }
        public List<int> solicitudes { get; set; } = [];
    }

    public class FrmCrApaCortesGarantiasIncluirRequest : FrmCrApaCortesGarantiasClaveRequest
    {
        public List<FrmCrApaCortesGarantiasDetalleDto> garantias { get; set; } = [];
    }
}
