namespace Galileo.Models.PRES
{
    public class AlertasTiposDto
    {
        public string cod_desviacion { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool? activa { get; set; }
        public bool? requiere_justificacion { get; set; }
        public int? orden_evaluacion { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
        public DateTime? modifica_fecha { get; set; }
        public string? modifica_usuario { get; set; } = string.Empty;
    }

    public class AlertasTiposLista
    {
        public int total { get; set; } = 0;
        public List<AlertasTiposDto> lista { get; set; } = new List<AlertasTiposDto>();
    }

    public class AlertasTiposDetalleDto
    {
        public int id_condicion { get; set; } = 0;
        public string cod_desviacion { get; set; } = string.Empty;
        public int grupo_condicion { get; set; } = 0; 
        public int orden_condicion { get; set; } = 0;
        public string campo_consulta { get; set; } = string.Empty;
        public string operador { get; set; } = string.Empty;
        public decimal? valor_inicial { get; set; }
        public decimal? valor_final { get; set; }
        public bool? activa { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
        public string modifica_usuario { get; set; } = string.Empty;
        public DateTime? modifica_fecha { get; set; }
    }

    public class AlertasTiposDetalleLista
    {
        public int total { get; set; } = 0;
        public List<AlertasTiposDetalleDto> lista { get; set; } = new();
    }

    public class AlertasTiposDetalleEliminarRequest
    {
        public int id_condicion { get; set; } = 0;
        public string cod_desviacion { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
    }

    public class AlertasTiposJustificacionDto
    {
        public string cod_tp_justificacion { get; set; } = string.Empty;
        public string id_justificacion { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool? activa { get; set; }
        public string registra_usuario { get; set; } = string.Empty;
        public DateTime? registra_fecha { get; set; }
        public string modifica_usuario { get; set; } = string.Empty;
        public DateTime? modifica_fecha { get; set; }
    }

    public class AlertasTiposJustificacionLista
    {
        public int total { get; set; } = 0;
        public List<AlertasTiposJustificacionDto> lista { get; set; } = new();
    }

    public class AlertasTiposJustificacionEliminarRequest
    {
        public string id_justificacion { get; set; } = string.Empty;
        public string cod_tp_justificacion { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
    }

    public static class AlertasTiposConst
    {
        public const string noExisteUsuario = "No existe el registro.";
    }
}