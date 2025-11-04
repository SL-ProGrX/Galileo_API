namespace PgxAPI.Models.Security
{
    public class RolesVincularDto
    {
        public int Index { get; set; }
        public string CodRol { get; set; } = string.Empty;
        public long? CodEmpresa { get; set; }
    }

    public class RolesObtenerDto
    {
        public string Cod_Rol { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public string Cliente { get; set; } = string.Empty;
        public DateTime Registro_Fecha { get; set; }
        public string Registro_Usuario { get; set; } = string.Empty;
    }

    public class RolInsertarDto
    {
        public string Cod_Rol { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public string Cliente { get; set; } = string.Empty;
        public DateTime Registro_Fecha { get; set; }
        public string Registro_Usuario { get; set; } = string.Empty;
    }

    public class ClientesObtenerDto
    {
        public string Cod_Empresa { get; set; } = string.Empty;
        public string Nombre_Corto { get; set; } = string.Empty;
        public string Nombre_Largo { get; set; } = string.Empty;
    }
}
