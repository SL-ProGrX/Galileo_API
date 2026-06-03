namespace Galileo.Models.ProGrX_Personas
{
    public class AfAfiliacionTramiteDto
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public DateTime? fechaIngreso { get; set; }
        public string cedular { get; set; } = string.Empty;
        public string reg_User { get; set; } = string.Empty;
        public DateTime? reg_Fecha { get; set; }
        public string estadoPersona { get; set; } = string.Empty;
    }

    public class AfAfiliacionTramiteFiltros
    {
        public int codInstitucion { get; set; }
        public string institucion { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string idAlterna { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public DateTime inicio { get; set; }
        public DateTime corte { get; set; }
    }
}
