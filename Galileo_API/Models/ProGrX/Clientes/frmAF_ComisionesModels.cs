namespace Galileo.Models.ProGrX.Clientes
{
    public class AfComisionDto
    {
        public required int cod_comision { get; set; }
        public DateTime? fecha { get; set; }
        public string? usuario { get; set; }
        public string? estado { get; set; }
        public decimal total { get; set; }
    }

    public class AfComisionPromotorData
    {
        public required int id_promotor { get; set; }
        public required string nombre { get; set; }
        public required string tipo { get; set; }
        public required int casos { get; set; }
        public required decimal monto { get; set; }
    }

    public class AfComisionPagoData
    {
        public required int id_promotor { get; set; }
        public required string nombre { get; set; }
        public required decimal monto { get; set; }
        public required string tipo_documento { get; set; }
        public string? cuenta_ahorros { get; set; }
        public required string nombre_contacto { get; set; }
    }

    public class AfComisionReporteData
    {
        public required string tipo_reporte { get; set; }
        public DateTime fecha_inicio { get; set; }
        public DateTime fecha_corte { get; set; }
        public bool chkFechas { get; set; }
        public required string _base { get; set; }
        public int remesa { get; set; }
        public bool chkRemesa { get; set; }
        public int promotor { get; set; }
        public bool chkPromotor { get; set; }
        public int banco { get; set; }
        public bool chkBanco { get; set; }
        public required string usuario { get; set; }
        public bool chkUsuario { get; set; }
        public required string reporte { get; set; }
        public bool chkMostrarSinComision { get; set; }
    }
}