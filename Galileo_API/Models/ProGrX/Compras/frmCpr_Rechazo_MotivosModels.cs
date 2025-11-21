namespace Galileo.Models.CPR
{
    public class CprRechazosMotivosLista
    {
        public int total { get; set; }
        public List<CprRechazosMotivosDto> lista { get; set; } = new List<CprRechazosMotivosDto>();
    }

    public class CprRechazosMotivosDto
    {
        public string cod_rechazo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool activo { get; set; }
        public Nullable<DateTime> registro_fecha { get; set; }
        public string? registro_usuario { get; set; }
        public Nullable<DateTime> modifica_fecha { get; set; }
        public string? modifica_usuario { get; set; }
        public bool isNew { get; set; }
    }
}