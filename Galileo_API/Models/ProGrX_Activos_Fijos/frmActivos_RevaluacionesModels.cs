namespace Galileo.Models.ProGrX_Activos_Fijos
{
    public class ActivosRevaluacionData
    {
        public int id_addret { get; set; }
        public string num_placa { get; set; } = string.Empty;
        public string cod_justificacion { get; set; } = string.Empty;
        public string justificacion { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public DateTime fecha { get; set; }
        public decimal monto { get; set; }  
        public int meses_calculo { get; set; } 
    }
}