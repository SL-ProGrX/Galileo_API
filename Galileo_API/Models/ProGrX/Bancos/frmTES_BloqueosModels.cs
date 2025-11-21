namespace Galileo.Models.ProGrX.Bancos
{
    public class TesBloqueoTransaccionDto
    {
        public int nsolicitud { get; set; }
        public int id_banco { get; set; }
        public string? tipo { get; set; }
        public string? codigo { get; set; }
        public string? beneficiario { get; set; }
        public decimal monto { get; set; }
        public DateTime fecha_solicitud { get; set; }
        public string? estado { get; set; }
        public string? detalle1 { get; set; }
        public string? detalle2 { get; set; }
        public string? detalle3 { get; set; }
        public string? detalle4 { get; set; }
        public string? detalle5 { get; set; }
        public string? user_solicita { get; set; }
        public string? user_hold { get; set; }
        public DateTime? fecha_hold { get; set; }
        public string? bancox { get; set; }
        public string? conceptox { get; set; }
        public string? unidadx { get; set; }
        public string? tipox { get; set; }
    }

    public class TesBloqueosFiltros
    {
        public bool todas_fechas { get; set; }
        public DateTime? fecha_inicio { get; set; }
        public DateTime? fecha_corte { get; set; }
        public bool todas_solicitudes { get; set; }
        public int? solicitud_inicio { get; set; }
        public int? solicitud_corte { get; set; }
        public string filtro { get; set; } = string.Empty;
        public int? pagina { get; set; } = 0;
        public int? paginacion { get; set; } = 30;
    }
}