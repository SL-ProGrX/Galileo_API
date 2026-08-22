namespace Galileo_API.Models.ProGrX_Conciliacion
{
    public class RastreoMovOpPeriodoData
    {
        public int Id_Per_Historico { get; set; }
        public int Anio { get; set; }
        public int Mes { get; set; }
    }

    public class RastreoMovOpSaldosData
    {
        public long? Operacion { get; set; }
        public string? Codigo { get; set; }
        public string? Identificacion { get; set; }
        public string? Nombre { get; set; }
        public decimal? Saldo_Inicial { get; set; }
        public decimal? Saldo_Final { get; set; }
        public decimal? Debitos { get; set; }
        public decimal? Creditos { get; set; }
        public decimal? Diferencia { get; set; }
    }

    public class RastreoMovOpSaldosRequest
    {
        public int? Id_Per_Historico { get; set; }
        public int? Lineas { get; set; }
        public bool? Diferencias { get; set; }
    }
}
