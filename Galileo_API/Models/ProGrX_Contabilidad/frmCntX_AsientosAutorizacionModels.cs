namespace Galileo_API.Models.ProGrX_Contabilidad
{
    public class CntXAsientoAutorizacionData
    {
        public string num_asiento { get; set; } = string.Empty;
        public string tipo_asiento { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public DateTime? fecha_asiento { get; set; }
        public decimal debitos { get; set; } = 0;
        public decimal creditos { get; set; } = 0;
    }
}
