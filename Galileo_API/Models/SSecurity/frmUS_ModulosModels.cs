namespace PgxAPI.Models.Security
{
    public class ModuloDto
    {
        public int Modulo { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public string KeyEnt { get; set; } = string.Empty;

    }
}
