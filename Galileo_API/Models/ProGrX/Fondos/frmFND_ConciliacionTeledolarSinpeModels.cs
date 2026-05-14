namespace Galileo.Models.ProGrX.Fondos
{
    public class FndConciliacionTeledolarSinpeParams
    {
        public int CodEmpresa { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaCorte { get; set; }
        public bool SoloDiferencias { get; set; }
    }

    public class FndConciliacionTeledolarSinpeResult
    {
        public DateTime Fecha { get; set; }
        public string Cedula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Producto { get; set; } = string.Empty;
        public string NumeroServicio { get; set; } = string.Empty;
        public int NumeroContrato { get; set; }
        public decimal MontoTeledolar { get; set; }
        public decimal DebitoCuentaCliente { get; set; }
        public decimal DiferenciaConciliada { get; set; }
        public string FondoNegativo { get; set; } = string.Empty;
        public int Estado { get; set; }
    }
}
