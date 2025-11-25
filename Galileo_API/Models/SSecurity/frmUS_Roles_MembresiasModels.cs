namespace Galileo.Models.Security
{
    public class RolesMembreciasInsertarDto
    {
        public int Ciente { get; set; }
        public string UsuarioLimita { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public int Limita { get; set; }
        public bool Acceso { get; set; }
    }

    public class UsuariosConsultaDto
    {
        public int UserID { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
    }

    public class UsuariosVinculadosConsultaDto
    {
        public int userID { get; set; }
        public string usuario { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
    }

    public class RolConsultaDto
    {
        public string COD_ROL { get; set; } = string.Empty;
        public string DESCRIPCION { get; set; } = string.Empty;
        public int Asignado { get; set; }
        public DateTime? REGISTRO_FECHA { get; set; }
        public string REGISTRO_USUARIO { get; set; } = string.Empty;
    }

    public class EstacionConsultaDto
    {
        public string ESTACION { get; set; } = string.Empty;
        public string DESCRIPCION { get; set; } = string.Empty;
        public int Asignado { get; set; }
        public DateTime? REGISTRO_FECHA { get; set; }
        public string REGISTRO_USUARIO { get; set; } = string.Empty;
    }

    public class HorarioConsultaDto
    {
        public string COD_HORARIO { get; set; } = string.Empty;
        public string DESCRIPCION { get; set; } = string.Empty;
        public int Asignado { get; set; }
        public DateTime? REGISTRO_FECHA { get; set; }
        public string REGISTRO_USUARIO { get; set; } = string.Empty;
    }

    public class UsuarioRolAsignaDto
    {
        public int? Cliente { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public string UsuarioRegistra { get; set; } = string.Empty;
        public char? TipoMov { get; set; }
    }

    public class UsuarioClienteAsigna
    {
        public int? Cliente { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public string UsuarioRegistra { get; set; } = string.Empty;
        public char? TipoMov { get; set; }
    }

    public class EstacionAsignaDto
    {
        public int? Cliente { get; set; }
        public string UsuarioLimita { get; set; } = string.Empty;
        public string Estacion { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public int? Acceso { get; set; }
    }

    public class LimitaAcceso
    {
        public int? Cliente { get; set; }
        public string UsuarioLimita { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public int? Limita { get; set; }
    }

    public class Limites
    {
        public int Estacion { get; set; } = 0;
        public int Horario { get; set; } = 0;
    }

    public class HorarioAsignaDto
    {
        public int? Cliente { get; set; }
        public string UsuarioLimita { get; set; } = string.Empty;
        public string Horario { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public int? Acceso { get; set; }
    }

}
