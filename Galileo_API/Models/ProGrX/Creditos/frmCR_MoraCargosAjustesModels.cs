namespace Galileo_API.Models.ProGrX.Creditos
{
    public class CrMoraCargosAjustesOperacionData
    {
        public int operacion { get; set; } = 0;
        public string cedula { get; set; } = string.Empty;
        public string codigo { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string num_documento { get; set; } = string.Empty;
        public int opex { get; set; } = 0;
        public string opex_descripcion { get; set; } = string.Empty;
        public string destinox { get; set; } = string.Empty;
        public string recursox { get; set; } = string.Empty;
        public string oficinax { get; set; } = string.Empty;
        public string garantiax { get; set; } = string.Empty;
        public int plazo_faltante { get; set; } = 0;
    }

    public class CrMoraCargosAjustesCuotasData
    {
        public int linea { get; set; } = 0;
        public string proceso { get; set; } = string.Empty;
        public DateTime? fecha_inicio { get; set; }
        public DateTime? fecha_corte { get; set; }
        public decimal int_cor { get; set; } = 0;
        public decimal int_mor { get; set; } = 0;
        public decimal principal { get; set; } = 0;
        public decimal cargos { get; set; } = 0;
        public string dias_mora { get; set; } = string.Empty;
    }

    public class CrMoraCargosAjustesCargosData
    {
        public int id_cargo { get; set; } = 0;
        public string proceso { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public decimal monto { get; set; } = 0;
        public decimal saldo { get; set; } = 0;
        public string notas { get; set; } = string.Empty;
        public int id_mora { get; set; } = 0;
    }

    public class CrMoraCargosAjustesFechaRequest
    {
        public int operacion { get; set; } = 0;
        public DateTime fecha_documento { get; set; } = DateTime.Now;
        public string usuario { get; set; } = string.Empty;
    }

    public class CrMoraCargosAjustesCuotasEliminarRequest
    {
        public int operacion { get; set; } = 0;
        public string usuario { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public List<CrMoraCargosAjustesCuotasData> lista { get; set; } = new();
    }

    public class CrMoraCargosAjustesCargosEliminarRequest
    {
        public int operacion { get; set; } = 0;
        public string usuario { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public List<CrMoraCargosAjustesCargosData> lista { get; set; } = new();
    }

    internal sealed class CrMoraCargosAjustesOperacionBaseData
    {
        public string codigo { get; set; } = string.Empty;
    }
}