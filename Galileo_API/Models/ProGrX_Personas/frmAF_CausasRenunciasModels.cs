namespace Galileo.Models.ProGrX_Personas
{
    public class AfCausasRenunciasData
    {
        public int id_causa { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public string tipo_apl { get; set; } = string.Empty;
        public decimal mortalidad { get; set; }
        public decimal ajuste_tasas { get; set; }
        public decimal liq_alterna { get; set; }
        public decimal tasa_planilla { get; set; }
        public decimal tasa_ventanilla { get; set; }
        public int institucion { get; set; }
        public string cod_plan { get; set; } = string.Empty;
        public int activo { get; set; }
    }
}