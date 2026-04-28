namespace Galileo_API.Models.ProGrX.Cobros
{
    public class CoAplExcInformeItemDto
    {
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class CoAplExcAplicacionF4Dto
    {
        public int id_aplicacion { get; set; }
        public DateTime? fecha { get; set; }
        public string usuario { get; set; } = string.Empty;
    }

    public class CoAplExcPersonaF4Dto
    {
        public string cedula { get; set; } = string.Empty;
        public string cedular { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
    }
}