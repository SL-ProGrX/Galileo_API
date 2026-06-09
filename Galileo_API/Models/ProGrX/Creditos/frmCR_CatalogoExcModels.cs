namespace Galileo_API.Models.ProGrX.Creditos
{
    public class CrCatalogoExcDisponibleModel
    {
        public int Mes { get; set; } = 0;
        public decimal Acumulado_Mes { get; set; } = 0;
        public decimal Acumulado_Porc { get; set; } = 0;
        public decimal CapGen { get; set; } = 0;
        public DateTime? Registro_Fecha { get; set; }
        public string Registro_Usuario { get; set; } = string.Empty;
        public DateTime? Modifica_Fecha { get; set; }
        public string Modifica_Usuario { get; set; } = string.Empty;
    }

    public class CrCatalogoExcDisponibleGuardarRequest
    {
        public int Mes { get; set; } = 0;
        public decimal Acumulado_Mes { get; set; } = 0;
        public decimal Acumulado_Porc { get; set; } = 0;
        public decimal CapGen { get; set; } = 0;
        public string Usuario { get; set; } = string.Empty;
    }

    public class CrCatalogoExcDisponibleEliminarRequest
    {
        public int Mes { get; set; } = 0;
        public string Usuario { get; set; } = string.Empty;
    }
}
