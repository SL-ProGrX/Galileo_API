namespace Galileo_API.Models.ProGrX.Creditos
{
    public sealed class CrOperacionCtaBulletData
    {
        public int operacion { get; set; } = 0;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string oficina { get; set; } = string.Empty;
        public decimal monto { get; set; } = 0;
        public decimal saldo_real { get; set; } = 0;
        public decimal saldo_base { get; set; } = 0;
        public int plazo_restante { get; set; } = 0;
        public decimal tasa_actual { get; set; } = 0;
        public decimal tasa_original { get; set; } = 0;
        public decimal cuota_bullet_actual { get; set; } = 0;
        public int ajuste_actual { get; set; } = 1;
        public decimal cuota_bullet { get; set; } = 0;
        public int ajuste { get; set; } = 1;
        public decimal cuota_minima { get; set; } = 0;
        public bool activa { get; set; } = false;
    }

    public sealed class CrOperacionCtaBulletGuardarRequest
    {
        public int operacion { get; set; } = 0;
        public decimal cuota_bullet { get; set; } = 0;
        public int ajuste { get; set; } = 1;
        public string usuario { get; set; } = string.Empty;
    }

    internal sealed class CrOperacionCtaBulletRow
    {
        public int operacion { get; set; } = 0;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string oficina { get; set; } = string.Empty;
        public decimal montoapr { get; set; } = 0;
        public decimal saldo { get; set; } = 0;
        public decimal interesv { get; set; } = 0;
        public decimal tasa_o { get; set; } = 0;
        public string? estado { get; set; }
        public string base_calculo { get; set; } = string.Empty;
        public int plazo_restante { get; set; } = 0;
        public decimal saldo_plan { get; set; } = 0;
        public decimal bullet_cta { get; set; } = 0;
        public int bullet_ajuste { get; set; } = 1;
    }
}