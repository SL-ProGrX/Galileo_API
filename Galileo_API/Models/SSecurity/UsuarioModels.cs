namespace PgxAPI.Models
{
    public class LoginRequest
    {
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }


    public class LoginResult
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public bool EsSuperAdmin { get; set; }
        public string Token { get; set; } = string.Empty;
        public List<LoginEmpresa>? EmpresaRol { get; set; }

        public bool BoolPasswordReset { get; set; }


    }

    public class LoginEmpresa
    {
        public int EmpresaId { get; set; }
        public int RoleId { get; set; }
        public string EmpresaDescripcion { get; set; } = string.Empty;
        public string RolDescripcion { get; set; } = string.Empty;
    }


    public class Usuario
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Movil { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int PasswordReset { get; set; }
        public string TokenId { get; set; } = string.Empty;
        public DateTime TokenDate { get; set; }
        public int Activo { get; set; }
        public DateTime RegistroFecha { get; set; }
        public string RegistroUsuario { get; set; } = string.Empty;
        public bool EsSuperAdmin { get; set; }
    }

    public class UsuarioDto : Usuario
    {
        public bool BoolPasswordReset { get; set; }
        public bool BoolActivo { get; set; }
    }

    public class UsuarioAdminDto : Usuario
    {
        public int RoleId { get; set; }
        public int EmpresaId { get; set; }
        public string EmpresaDescripcion { get; set; } = string.Empty;
        public string RolDescripcion { get; set; } = string.Empty;
    }

    public class UsuarioInfoDto : Usuario
    {
        public List<LoginEmpresa>? EmpresaRol { get; set; }
    }

    public class LoginDbResult
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public int EmpresaId { get; set; }
        public string EmpresaDescripcion { get; set; } = string.Empty;
        public string RolDescripcion { get; set; } = string.Empty;
        public bool EsSuperAdmin { get; set; }
        public int PasswordReset { get; set; }
    }

    public class UsuarioInfoDTO
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Movil { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int PasswordReset { get; set; }
        public int Activo { get; set; }

    }

    public class UsuarioCambioContrasenaDTO
    {
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string NuevoPassword { get; set; } = string.Empty;
        public string ConfirmacionPassword { get; set; } = string.Empty;
    }

    public class ErrorUsuarioDTO
    {
        public int Code { get; set; } = 0;
        public string Description { get; set; } = string.Empty;
    }

    public class UsuarioContrasenaBitacorasRespuestaDTO
    {
        public int IdKeyLog { get; set; }
        public int UserId { get; set; }
        public string KeyCode { get; set; } = string.Empty;
        public DateTime RegistroFecha { get; set; }
        public string RegistroUsuario { get; set; } = string.Empty;
    }

    public class UsuarioBitacorasRespuestaDTO
    {
        public int LogAccountId { get; set; }
        public int UserId { get; set; }
        public int MovTypeId { get; set; }
        public string Movimiento { get; set; } = string.Empty;
        public string Detalle { get; set; } = string.Empty;
        public DateTime RegistroFecha { get; set; }
        public string RegistroUsuario { get; set; } = string.Empty;
    }


    public class UsuarioEmpresaRolDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public int EmpresaId { get; set; }
        public string EmpresaDescripcion { get; set; } = string.Empty;
        public string RolDescripcion { get; set; } = string.Empty;
    }

    public class UsuarioRolEmpresaAgregarDto
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public int EmpresaId { get; set; }
        public string RegistroUsuario { get; set; } = string.Empty;

    }


    public class UsuarioCuentaRevisarDto
    {
        public string Usuario { get; set; } = string.Empty;
        public bool Bloqueo { get; set; } = false;
        public bool BloqueoI { get; set; } = false;
        public bool CambioLogon { get; set; } = false;
        public bool CuentaCaduca { get; set; } = false;
        public bool Admin { get; set; } = false;
        public bool CambioContrasena { get; set; } = false;
        public string Notas { get; set; } = string.Empty;
        public string AppName { get; set; } = string.Empty;
        public string AppVersion { get; set; } = string.Empty;
        public string UsuarioMovimiento { get; set; } = string.Empty;
        public string Maquina { get; set; } = string.Empty;
    }


    public class UsuarioCuentaMovimientoRequestDto
    {
        public string Usuario { get; set; } = string.Empty;
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaCorte { get; set; }
        public string? Estacion { get; set; } = null;
        public string ListaCodTransacciones { get; set; } = string.Empty;
        public string? AppName { get; set; } = null;
        public string? AppVersion { get; set; } = null;
        public string? UsuarioBusqueda { get; set; } = null;
        public string Revision { get; set; } = string.Empty;
        public bool RevisionInd { get; set; } = false;
    }

    public class UsuarioCuentaMovimientoResultDto
    {
        public bool Revisado { get; set; } = false;
        public DateTime? FechaMovimiento { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Notas { get; set; } = string.Empty;
        public string AppName { get; set; } = string.Empty;
        public string AppVersion { get; set; } = string.Empty;
        public string Equipo { get; set; } = string.Empty;
        public string UsuarioMovimiento { get; set; } = string.Empty;
        public string? RevisadoUsuario { get; set; }
        public DateTime? RevisadoFecha { get; set; }
        public int SeqId { get; set; } = 0;
        public string CodTransaccion { get; set; } = string.Empty;
        public string EquipoMAC { get; set; } = string.Empty;
    }

    public class LogonUpdateDataDto
    {
        public string NombreUsuario { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string EMAIL { get; set; } = string.Empty;
        public string TEL_CELL { get; set; } = string.Empty;
    }


}
