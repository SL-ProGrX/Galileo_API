namespace Galileo_API.Models.ProGrX_Comites
{
    public class AfCdParametroDto
    {
        public int Cod_Parametro { get; set; }
        public string? Detalle { get; set; }
        public string? Tipo { get; set; }
        public string? Valor { get; set; }
        public string? Notas { get; set; }
        public DateTime? Registro_Fecha { get; set; }
        public string? Registro_Usuario { get; set; }
    }

    public class AfCdParametroUpdateDto
    {
        public int Cod_Parametro { get; set; }
        public string Valor { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
    }
}
