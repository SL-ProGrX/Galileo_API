namespace Galileo_API.Models.ProGrX.Creditos
{
    public class CrGarantiaTiposData
    {
        public string garantia { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string formulario { get; set; } = string.Empty;
        public bool maximos_utiliza { get; set; } = false;
        public decimal maximos_monto { get; set; } = 0;
        public string prioridad { get; set; } = string.Empty;
        public string cta_mask { get; set; } = string.Empty;
        public string cta_desc { get; set; } = string.Empty;
        public decimal porc_mitigador { get; set; } = 0;
        public int ref_plazo { get; set; } = 0;
        public decimal ref_tasa { get; set; } = 0;
        public bool v_disponible { get; set; } = false;
    }
}
