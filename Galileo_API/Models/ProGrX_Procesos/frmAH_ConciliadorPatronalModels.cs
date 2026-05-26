namespace Galileo.Models.AH
{
    public class FrmAhConciliadorPatronalHistoricoDto
    {
        public string identificacion { get; set; } = string.Empty;
        public string id_alterna { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public decimal patronal { get; set; }
    }

    public class FrmAhConciliadorPatronalConciliacionDto
    {
        public string identificacion { get; set; } = string.Empty;
        public string id_alterna { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public decimal patronal { get; set; }
    }

    public class FrmAhConciliadorPatronalResultadoDto
    {
        public string identificacion { get; set; } = string.Empty;
        public string id_alterna { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public decimal patronal { get; set; }
        public decimal aporte_registrado { get; set; }
        public decimal diferencia { get; set; }
    }

    public class FrmAhConciliadorPatronalCargadoRequest
    {
        public int cod_institucion { get; set; }
        public DateTime fecha_corte { get; set; }
        public string tipo_analisis { get; set; } = string.Empty;
        public string registro_usuario { get; set; } = string.Empty;
        public List<FrmAhConciliadorPatronalHistoricoDto> registros { get; set; } = new();
    }

    public class FrmAhConciliadorPatronalAplicarResponse
    {
        public string accion { get; set; } = string.Empty;
        public string mensaje { get; set; } = string.Empty;
        public int total_registros { get; set; }
    }

    public class FrmAhConciliadorPatronalConciliacionRequest
    {
        public int cod_institucion { get; set; }
        public DateTime fecha_corte { get; set; }
        public string localizados { get; set; } = string.Empty;
        public string registro_usuario { get; set; } = string.Empty;
        public List<FrmAhConciliadorPatronalHistoricoDto> registros { get; set; } = new();
    }

    public class FrmAhConciliadorPatronalResultadosRequest
    {
        public int cod_institucion { get; set; }
        public DateTime fecha_corte { get; set; }
        public string resultado { get; set; } = string.Empty;
        public string registro_usuario { get; set; } = string.Empty;
        public List<FrmAhConciliadorPatronalHistoricoDto> registros { get; set; } = new();
    }
}