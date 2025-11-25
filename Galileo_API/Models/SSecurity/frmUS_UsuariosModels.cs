namespace Galileo.Models.Security
{
    public class UsuarioModel
    {
        public string UserName { get; set; } = string.Empty;
        public int UserId { get; set; } = 0;
        public string Identificacion { get; set; } = string.Empty;
        public bool? ContabilizaCobranza { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public DateTime? FechaIngreso { get; set; }
        public DateTime? FechaUltimo { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string TelCelular { get; set; } = string.Empty;
        public string TelTrabajo { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Notas { get; set; } = string.Empty;
        public string UsuarioRegistro { get; set; } = string.Empty;
        public bool? ModoEdicion { get; set; }
        public int EmpresaId { get; set; } = 0;
        public string NombreEmpresa { get; set; } = string.Empty;
        public bool? tfa_ind { get; set; }
        public string tfa_metodo { get; set; } = string.Empty;
        public DateTime? tfa_vence { get; set; }
        public string tfa_activo { get; set; } = string.Empty;
    }

    public class UsuarioClienteAsignaDto
    {
        public int? CodigoEmpresa { get; set; }
        public string NombreEmpresa { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public string UsuarioRegistra { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string AppName { get; set; } = string.Empty;
        public string AppVersion { get; set; } = string.Empty;
    }

    public class UsuarioClienteDto
    {
        public string Cod_Empresa { get; set; } = string.Empty;
        public string Nombre_Corto { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public bool Seleccionado { get; set; } = false;
    }

    public class TipoTransaccionBitacora
    {
        public string Codigo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }

    public class UsuarioCuentaBitacora
    {
        public int SeqId { get; set; } = 0;
        public string Usuario { get; set; } = string.Empty;
        public DateTime MovFecha { get; set; }
        public string MovUser { get; set; } = string.Empty;
        public string Notas { get; set; } = string.Empty;
        public string AppName { get; set; } = string.Empty;
        public string AppVersion { get; set; } = string.Empty;
        public string Equipo { get; set; } = string.Empty;
        public string RevisadoUsuario { get; set; } = string.Empty;
        public DateTime RevisadoFecha { get; set; }
        public string IPAddress { get; set; } = string.Empty;
        public string EquipoMAC { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }

    public class UsuarioBitacoraRequest
    {
        public string Usuario { get; set; } = string.Empty;
        public int Lineas { get; set; } = 0;
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaCorte { get; set; }
        public string CodTransac { get; set; } = string.Empty;
    }

    public class UsuarioClienteRolDto
    {
        public string CodigoRol { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Asignado { get; set; }
        public DateTime RegistroFecha { get; set; }
        public string RegistroUsuario { get; set; } = string.Empty;
    }

    public class UsuarioClienteRolAsignaDto
    {
        public string CodigoRol { get; set; } = string.Empty;
        public int? CodigoEmpresa { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public string UsuarioRegistra { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
    }
}
