namespace Galileo.Models.ProGrX.Clientes
{
    public class AfCartasNoCotizantesFiltros
    {
        public required string tipoDocumento { get; set; }

        public required string meses { get; set; }
        public int mesesNoCotizar { get; set; }
        public DateTime fechaIngreso { get; set; }
        public required string mora { get; set; }
        public int cuotaMora { get; set; }

        public bool chkCreditos { get; set; }
        public bool chkEmail { get; set; }
    }

    public class AfCartasNoCotizantesData
    {
        public required string cedula { get; set; }
        public required string nombre { get; set; }
        public int meses { get; set; }
        public decimal saldos { get; set;}
        public decimal intCor { get; set; }
        public decimal intMor { get; set; }
        public int cuotas { get; set; }
    }
}
