namespace Galileo.Models.ProGrX_Activos_Fijos
{
    public class ActivosParametrosData
    {
        public string enlace_conta { get; set; } = string.Empty; 
        public string enlace_sifc { get; set; } = string.Empty;
        public int registro_periodo_cerrado { get; set; }
        public string nombre_empresa { get; set; } = string.Empty;
        public int forzar_tipoactivo { get; set; }
        public int registrocompras { get; set; }
        public string tipo_anio { get; set; } = string.Empty;
        public int inicio_anio { get; set; } 
        public int cod_empresa { get; set; }
    }

}