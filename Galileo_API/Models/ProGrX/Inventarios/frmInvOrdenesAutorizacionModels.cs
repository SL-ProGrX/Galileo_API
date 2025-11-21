namespace Galileo.Models.INV
{
    public class ResolucionTransaccionDto
    {
        public string? Cod_Orden { get; set; }
        public string? Tipo_Orden { get; set; }
        public int? Total { get; set; } = 0;
        public string? genera_user { get; set; } 
        public DateTime? Fecha { get; set; }
        public string? Causa { get; set; } 
        public string? Nota { get; set; } 
        public string? proceso { get; set; } 
        public bool? seleccionado { get; set; } = false;
    }

    public class ResolucionTransaccionFiltros
    {
        public string? fecha { get; set; }
        public string? fecha_inicio { get; set; }
        public string? fecha_corte { get; set; }
        public string? tipo { get; set; }
    }
}