namespace PgxAPI.Models.ProGrX_Activos_Fijos
{
    public class ActivosPrincipalesData
    {
        public string num_placa { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string vida_util_en { get; set; } = string.Empty;
        public int vida_util { get; set; }
        public string tipo_activo { get; set; } = string.Empty;
        public string tipo_activo_desc { get; set; } = string.Empty;
    }
}