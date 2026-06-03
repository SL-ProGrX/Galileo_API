namespace Galileo.Models.ProGrX_Personas
{
    public class AfNoticaNoCotizantesFiltros
    {
        public int? codInstitucion { get; set; }
        public int informe { get; set; }
        public int rangoId { get; set; }
        public int aviso { get; set; }
    }

    public class AfAsociadosSinAportesDto
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public DateTime? fechaingreso { get; set; }
        public int id_promotor { get; set; }
        public int dias_afiliado { get; set; }
        public string estado_asociado { get; set; } = string.Empty;
        public string institucion { get; set; } = string.Empty;
        public decimal aporte_obrero { get; set; }
        public decimal aporte_patronal { get; set; }
        public decimal capitalización { get; set; }
        public decimal capitalizacion { get; set; }
        public string primer_deducción { get; set; } = string.Empty;
        public string provincia { get; set; } = string.Empty;
        public string provincia_desc { get; set; } = string.Empty;
        public string canton { get; set; } = string.Empty;
        public string canton_desc { get; set; } = string.Empty;
        public string distrito { get; set; } = string.Empty;
        public string distrito_desc { get; set; } = string.Empty;
        public string direccion { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string? email_02 { get; set; }
        public string? telefono_habitacion { get; set; }
        public string? telefono_trabajo { get; set; }
        public string? telefono_celular { get; set; }
        public decimal creditos_monto { get; set; }
        public decimal creditos_saldo { get; set; }
        public decimal creditos_cuota { get; set; }
        public decimal morosidad { get; set; }
        public decimal fondos_acumulado { get; set; }
        public string promotor_desc { get; set; } = string.Empty;
        public string? up { get; set; }
        public string? up_desc { get; set; }
        public DateTime? ultimoaporteobrero { get; set; }
        public DateTime? ultimoaportepatronal { get; set; }
        public DateTime? fecha_suspendido { get; set; }
        public DateTime? fecha_activo { get; set; }
        public int? dias_aporte_obrero { get; set; }
        public int? dias_aporte_patronal { get; set; }
    }
}
