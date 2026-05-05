namespace Galileo.Models.ProGrX.Cobros
{
    public class CoComisionesDocumentosData
    {
        public string tipo_documento { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string asignado { get; set; } = string.Empty;
        public DateTime registro_fecha { get; set; }  
        public string registro_usuario { get; set; } = string.Empty;
        public bool activo => asignado != "No-ASG";

    }
}