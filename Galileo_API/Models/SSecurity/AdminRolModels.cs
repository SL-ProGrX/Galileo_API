namespace Galileo.Models.Security
{
    public class UsuarioPlataforma
    {
        public int UserId { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
    }

    public class UsuarioAdmin
    {
        public int UserId { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public int Activo { get; set; }
        public DateTime RegistroFecha { get; set; }
        public string RegistroUsuario { get; set; } = string.Empty;
    }

    public class ClienteAsignado
    {
        public int Cod_Empresa { get; set; }
        public string Nombre_Corto { get; set; } = string.Empty;
        public string Nombre_Largo { get; set; } = string.Empty;
        public bool Asignado { get; set; }
    }

    public class AdminLocalRoles
    {
        public string Usuario { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public DateTime RegistroFecha { get; set; }
        public string RegistroUsuario { get; set; } = string.Empty;
        public bool R_Local_Grants { get; set; }
        public bool R_Local_Users { get; set; }
        public bool R_Global_Dir_Search { get; set; }
        public bool R_Local_Key_Reset { get; set; }
        public bool R_Admin_Review { get; set; }
        public DateTime Salida_Fecha { get; set; }
        public string Salida_Usuario { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }

    public class AdminLocalRolesCliente
    {
        public string Nombre_Corto { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public int Cod_Empresa { get; set; }
        public DateTime RegistroFecha { get; set; }
        public string RegistroUsuario { get; set; } = string.Empty;
        public bool R_Local_Grants { get; set; }
        public bool R_Local_Users { get; set; }
        public bool R_Local_Key_Reset { get; set; }
        public bool R_Global_Dir_Search { get; set; }
        public bool R_Admin_Review { get; set; }
        public DateTime Salida_Fecha { get; set; }
        public string Salida_Usuario { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }

    public class AdminLocalInsert
    {
        public string Usuario { get; set; } = string.Empty;
        public string Mov { get; set; } = string.Empty;
        public string UsuarioRegister { get; set; } = string.Empty;
        public bool? R_Local_Grants { get; set; }
        public bool? R_Local_Users { get; set; }
        public bool? R_Local_Key_Reset { get; set; }
        public bool? R_Global_Dir_Search { get; set; }
        public bool? R_Admin_Review { get; set; }
        public bool? Propaga_Clientes { get; set; }
    }

    public class AdminLocalRolesInsert
    {
        public string Usuario { get; set; } = string.Empty;
        public int? ClienteId { get; set; }
        public string Mov { get; set; } = string.Empty;
        public string UsuarioRegister { get; set; } = string.Empty;
        public bool? R_Local_Grants { get; set; }
        public bool? R_Local_Users { get; set; }
        public bool? R_Local_Key_Reset { get; set; }
        public bool? R_Global_Dir_Search { get; set; }
        public bool? R_Admin_Review { get; set; }
    }
}