namespace PgxAPI.Models
{
    public class ModuloDTO
    {
        public int Modulo { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public string KeyEnt { get; set; } = string.Empty;

    }

    public class ErrorModuloDTO
    {
        public int Code { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
