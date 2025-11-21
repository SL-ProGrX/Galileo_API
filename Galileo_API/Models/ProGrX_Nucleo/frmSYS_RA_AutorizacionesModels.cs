namespace Galileo.Models.ProGrX_Nucleo
{
    public class SysAutorizacionesData
    {
        public int persona_id { get; set; } 
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string tipodesc { get; set; } = string.Empty;
        public string tipo_id { get; set; } = string.Empty;
        public string estadodesc { get; set; } = string.Empty;
        public string usuario_autorizado { get; set; } = string.Empty;
        public string nombre_autorizado { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public int horas { get; set; }

    }
}
