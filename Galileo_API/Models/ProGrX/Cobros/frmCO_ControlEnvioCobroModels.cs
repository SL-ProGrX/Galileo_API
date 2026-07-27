namespace Galileo.Models.ProGrX.Cobros
{
    public sealed class CoControlEnvioCobroPendienteData
    {
        public int cod_seg { get; set; } = 0;
        public DateTime? fecha { get; set; }
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public decimal monto { get; set; } = 0;
        public string cod_gestion { get; set; } = string.Empty;
        public string gestion_x { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public string codigo_referencia { get; set; } = string.Empty;
    }
}