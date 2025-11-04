namespace PgxAPI.Models.INV
{
    public class TransacQryParametros
    {
        public string? Estado { get; set; } = string.Empty;
        public string? Tipo { get; set; } = string.Empty;
        public string? TipoFecha { get; set; } = string.Empty;
        public string? FechaInicio { get; set; }
        public string? FechaCorte { get; set; }
        public string? TipoUsuario { get; set; } = string.Empty;
        public string? Usuario { get; set; } = string.Empty;
        public string? vfiltro { get; set; } = string.Empty;
        public int? pagina { get; set; }
        public int? paginacion { get; set; }
    }

    public class TransacQryDataList
    {
        public int Total { get; set; }
        public List<TransacQryData> Transacciones { get; set; } = new List<TransacQryData>();
    }

    public class TransacQryData
    {
        public string Boleta { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string Documento { get; set; } = string.Empty;
        public string Notas { get; set; } = string.Empty;
        public string Genera_User { get; set; } = string.Empty;
        public DateTime Genera_Fecha { get; set; }
        public string Autoriza_User { get; set; } = string.Empty;
        public DateTime Autoriza_Fecha { get; set; }
        public string Procesa_User { get; set; } = string.Empty;
        public DateTime Procesa_Fecha { get; set; }

    }
}