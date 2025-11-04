namespace PgxAPI.Models.Security
{
    public class FormularioModel
    {
        public string Formulario { get; set; } = String.Empty;
        public string Descripcion { get; set; } = String.Empty;
    }

    public class FormularioDto
    {
        public string Nombre { get; set; } = String.Empty;
        public string Descripcion { get; set; } = String.Empty;
        public int ModuloId { get; set; } = 0;
        public string Usuario { get; set; } = String.Empty;
    }
}
