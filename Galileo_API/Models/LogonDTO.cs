namespace PgxAPI.Models
{
    public class IntentosObtenerDto
    {
        public int KEY_INTENTOS { get; set; }
        public int TIME_LOCK { get; set; }
    }

    public class LoginObtenerDto
    {
        public string Usuario { get; set; } = string.Empty;
        public string Clave { get; set; } = string.Empty;
    }

    public class ClientesEmpresasObtenerDto
    {
        public string CodEmpresa { get; set; } = string.Empty;
        public string NombreCorto { get; set; } = string.Empty;
        public string Cod_Empresa { get; set; } = string.Empty;
        public string Nombre_Corto { get; set; } = string.Empty;
        public string Nombre_Largo { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
    }

    public class TfaData
    {
        public string email { get; set; } = string.Empty;
        public string cellphone { get; set; } = string.Empty;
        public bool tfa_ind { get; set; }
        public string tfa_metodo { get; set; } = string.Empty;
    }

    public class TfaDatosCorreo
    {
        public string codigo { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
    }
}
