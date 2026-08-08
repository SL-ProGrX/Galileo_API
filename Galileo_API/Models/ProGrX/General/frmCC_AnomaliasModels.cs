namespace Galileo_API.Models.ProGrX.General
{
    public class CcAnomaliaFiltroDto
    {
        public decimal Monto { get; set; }
        public string? Linea { get; set; }
        public string? Destino { get; set; }
        public int? Institucion { get; set; }
    }

    public class CcAnomaliaCtaDerivadaFiltroDto
    {
        public decimal Monto { get; set; }
    }

    public class CcAnomaliaCreditoItemDto
    {
        public string Codigo { get; set; } = string.Empty;
        public int Id_Solicitud { get; set; }
        public string Cedula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public decimal Saldo { get; set; }
        public string Opex { get; set; } = string.Empty;
        public string Proceso { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string? Estadosol { get; set; }
        public decimal? MoraFinanciera { get; set; }
        public string Institucion { get; set; } = string.Empty;
        public string LineaDesc { get; set; } = string.Empty;
        public string Destino { get; set; } = string.Empty;
    }

    public class CcAnomaliaCtaDerivadaItemDto
    {
        public int Id_Solicitud { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Cedula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public decimal Saldo { get; set; }
        public int Num_Cuota { get; set; }
        public decimal Monto { get; set; }
        public string Descripcion { get; set; } = string.Empty;
    }
}
