namespace Galileo_API.Models.ProGrX_Pasivos
{
    public sealed class FrmCrApaOperacionRenumeraOperacionDto
    {
        public string operacion { get; set; } = string.Empty;
        public decimal monto { get; set; } = 0;
        public decimal saldo { get; set; } = 0;
        public DateTime? fecha_formaliza { get; set; }
    }

    public sealed class FrmCrApaOperacionRenumeraAplicarRequest
    {
        public string cod_acreedor { get; set; } = string.Empty;
        public string operacion { get; set; } = string.Empty;
        public string operacion_nueva { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
    }

    public sealed class FrmCrApaOperacionRenumeraResultadoDto
    {
        public string mensaje { get; set; } = string.Empty;
    }
}