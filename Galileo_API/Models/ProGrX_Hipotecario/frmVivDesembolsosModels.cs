namespace Galileo_API.Models.ProGrX_Hipotecario
{
    public class VivDesembolsoHeaderDto
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;

        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;

        public decimal? Bruto { get; set; }
        public decimal? IntAcumulado { get; set; }
        public decimal? IntSDisponible { get; set; }
        public decimal? GiroMaximo { get; set; }

        public decimal? monto_girado { get; set; }
    }

    public class VivDesembolsoDto
    {
        public string beneficiario { get; set; } = string.Empty;

        public decimal monto { get; set; }
        public decimal disponible { get; set; }

        public DateTime? fechacorte { get; set; }

        public string usuario { get; set; } = string.Empty;
    }

    public class VivDesembolsoPendienteDto
    {
        public string linea { get; set; } = string.Empty;
        public int? idcontacto { get; set; }
        public int? garantia { get; set; }
        public string cedula { get; set; } = string.Empty;

        public string concepto { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string destipo { get; set; } = string.Empty;

        public string beneficiario { get; set; } = string.Empty;

        public decimal monto { get; set; }
        public decimal descuento { get; set; }
        public decimal montogiro { get; set; }

        public string cuenta { get; set; } = string.Empty;
        public string aplicainteres { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;

        public DateTime? fecha { get; set; }
    }
}
