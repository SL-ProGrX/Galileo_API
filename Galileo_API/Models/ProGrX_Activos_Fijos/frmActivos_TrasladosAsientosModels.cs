namespace Galileo.Models.ProGrX_Activos_Fijos
{
    public class ActivosTrasladoAsientosDto
    {
        public string num_asiento { get; set; } = string.Empty;
        public string tipo_asiento { get; set; } = string.Empty;
        public DateTime fecha_asiento { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public int anio { get; set; }
        public int mes { get; set; }
        public int cod_contabilidad { get; set; }
    }
    public class ActivosTrasladoAsientoRequest
    {
        public int cod_contabilidad { get; set; }
        public string tipo_asiento { get; set; } = string.Empty;
        public string num_asiento { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
    }
}
