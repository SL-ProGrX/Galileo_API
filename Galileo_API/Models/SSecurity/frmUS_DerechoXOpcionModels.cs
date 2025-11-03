namespace PgxAPI.Models
{
    public class ModuloResultDto
    {
        public int Modulo { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public string Keyent { get; set; } = string.Empty;
    }

    public class FormularioResultDto
    {
        public string Formulario { get; set; } = string.Empty;
        public int Modulo { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public DateTime RegistroFecha { get; set; }
        public string RegistroUsuario { get; set; } = string.Empty;
    }

    public class OpcionResultDto
    {
        public int Cod_Opcion { get; set; }
        public string Formulario { get; set; } = string.Empty;
        public int Modulo { get; set; }
        public string Opcion { get; set; } = string.Empty;
        public string Opcion_Descripcion { get; set; } = string.Empty;
        public DateTime Registro_Fecha { get; set; }
        public string Registro_Usuario { get; set; } = string.Empty;
    }

    public class DatosResultDto
    {
        public string cod_rol { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
    }

    public class OpcionRolRequestDto
    {
        public int opcion { get; set; }
        public string rol { get; set; } = string.Empty;
        public char tipo { get; set; }
        public string usuario { get; set; } = string.Empty;
        public bool check { get; set; }
    }

}
