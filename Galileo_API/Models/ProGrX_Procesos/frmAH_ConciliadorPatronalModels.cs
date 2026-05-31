namespace Galileo.Models.AH
{
    public class FrmAhConciliadorPatronalHistoricoDto
    {
        public string identificacion { get; set; } = string.Empty;
        public string id_alterna { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public decimal patronal { get; set; } = 0;
    }

    public class FrmAhConciliadorPatronalConciliacionDto
    {
        public string identificacion { get; set; } = string.Empty;
        public string id_alterna { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public decimal patronal { get; set; } = 0;
    }

    public class FrmAhConciliadorPatronalResultadoDto
    {
        public string identificacion { get; set; } = string.Empty;
        public string id_alterna { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public decimal patronal { get; set; } = 0;
        public decimal aporte_registrado { get; set; } = 0;
        public decimal diferencia { get; set; } = 0;
    }

    public class FrmAhConciliadorPatronalCargadoRequest
    {
        public int cod_institucion { get; set; } = 0;
        public DateTime fecha_corte { get; set; } = DateTime.MinValue;
        public string tipo_analisis { get; set; } = string.Empty;
        public string registro_usuario { get; set; } = string.Empty;
        public List<FrmAhConciliadorPatronalHistoricoDto> registros { get; set; } = new();
    }

    public class FrmAhConciliadorPatronalAplicarResponse
    {
        public string accion { get; set; } = string.Empty;
        public string mensaje { get; set; } = string.Empty;
        public int total_registros { get; set; } = 0;
    }

    public class FrmAhConciliadorPatronalConciliacionRequest
    {
        public int cod_institucion { get; set; } = 0;
        public DateTime fecha_corte { get; set; } = DateTime.MinValue;
        public string localizados { get; set; } = string.Empty;
        public string registro_usuario { get; set; } = string.Empty;
        public List<FrmAhConciliadorPatronalHistoricoDto> registros { get; set; } = new();
    }

    public class FrmAhConciliadorPatronalResultadosRequest
    {
        public int cod_institucion { get; set; } = 0; 
        public DateTime fecha_corte { get; set; } = DateTime.MinValue;
        public string resultado { get; set; } = string.Empty;
        public string registro_usuario { get; set; } = string.Empty;
        public List<FrmAhConciliadorPatronalHistoricoDto> registros { get; set; } = new();
    }
}