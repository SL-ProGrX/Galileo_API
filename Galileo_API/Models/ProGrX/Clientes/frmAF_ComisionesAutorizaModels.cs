namespace Galileo.Models.ProGrX.Clientes
{
    public class ComisionAutorizaData
    {
        public int IdBoleta { get; set; }
        public string Cedula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public int IdPromotor { get; set; }
        public string PromotorX { get; set; } = string.Empty;
        public DateTime FechaIngreso { get; set; }
        public int Comision_Autoriza { get; set; }
        public int AutorizacionX { get; set; }
        public string EstadoActual { get; set; } = string.Empty;
        public DateTime? Fecha_Comision { get; set; }
        public string Reg_User { get; set; } = string.Empty;
        public string Autoriza_Comision_Notas { get; set; } = string.Empty;
    }

    public class ComisionAutorizaFiltroDto
    {
        public DateTime Inicio { get; set; }
        public DateTime Corte { get; set; }
        public bool ChkAportes { get; set; }
        public bool ChkPromotor { get; set; }
        public bool ChkUsuarios { get; set; }
        public int? IdPromotor { get; set; }
        public string? Usuario { get; set; }
        public int? Autorizado { get; set; }
    }
}