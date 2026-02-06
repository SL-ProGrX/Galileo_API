namespace Galileo_API.Models.ProGrX_Contabilidad
{
    public class CntXCierreData
    {
        public int id_cierre { get; set; } = 0;
        public int inicio_anio { get; set; } = 0;
        public int inicio_mes { get; set; } = 0;
        public int corte_anio { get; set; } = 0;
        public int corte_mes { get; set; } = 0;
        public string descripcion { get; set; } = string.Empty;
        public string cuenta_ganper { get; set; } = string.Empty;
        public string cuenta_utilidad { get; set; } = string.Empty;
        public string cuenta_imprenta { get; set; } = string.Empty;
        public decimal impuesto_renta { get; set; } = 0;
        public bool activo { get; set; } = false;
    }
}
