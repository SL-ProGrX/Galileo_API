namespace Galileo.Models.ProGrX_Personas
{
    public class AfSuspendidosConsultaFiltros
    {
        public int evento { get; set; }
        public bool todas_fechas { get; set; }
        public DateTime inicio { get; set; }
        public DateTime corte { get; set; }
        public string Cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
    }

    public class AfSuspendidosConsultaDto
    {
        public string Cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public DateTime fechaingreso { get; set; }
        public int id_promotor { get; set; }
        public int dias_afiliado { get; set; }
        public string EstadoAsociado { get; set; } = string.Empty;
        public string institucion { get; set; } = string.Empty;
        public decimal aporte_obrero { get; set; }
        public decimal aporte_patronal { get; set; }
        public decimal capitalización { get; set; }
        public decimal capitalizacion { get; set; }
        public decimal primer_deducción { get; set; }
        public string provincia { get; set; } = string.Empty;
        public string provincia_desc { get; set; } = string.Empty;
        public string canton { get; set; } = string.Empty;
        public string canton_desc { get; set; } = string.Empty;
        public string distrito { get; set; } = string.Empty;
        public string distrito_desc { get; set; } = string.Empty;
        public string direccion { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string email_02 { get; set; } = string.Empty;
        public string telefono_habitacion { get; set; } = string.Empty;
        public string telefono_trabajo { get; set; } = string.Empty;
        public string telefono_celular { get; set; } = string.Empty;
        public decimal creditos_monto { get; set; }
        public decimal creditos_saldo { get; set; }
        public decimal creditos_cuota { get; set; }
        public decimal morosidad { get; set; }
        public decimal fondos_acumulado { get; set; }
        public string promotor_desc { get; set; } = string.Empty;
        public string up { get; set; } = string.Empty;
        public string up_desc { get; set; } = string.Empty;
        public DateTime ultimoaporteobrero { get; set; }
        public DateTime ultimoaportepatronal { get; set; }
        public DateTime? fecha_suspendido { get; set; }
        public DateTime? fecha_activo { get; set; }
        public int dias_aporte_obrero { get; set; }
        public int dias_aporte_patronal { get; set; }
    }
}
