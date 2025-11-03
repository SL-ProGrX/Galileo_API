namespace PgxAPI.Models.CxP
{
    public class ParametrosDto
    {
        public string Cod_Parametro { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Valor { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string Visible { get; set; } = string.Empty;
        public string Notas { get; set; } = string.Empty;
        public DateTime? Inicio_Fecha { get; set; }
        public DateTime? Modifica_Fecha { get; set; }
        public string? Modifica_Usuario { get; set; }
    }

}
