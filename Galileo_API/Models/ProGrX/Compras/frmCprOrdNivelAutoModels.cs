namespace PgxAPI.Models.CPR
{
    public class UsuariosAutorizaData
    {
        public string nombre { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public DateTime? fecha { get; set; }
        public string? usuario { get; set; }
        public DateTime? fecha_asignacion { get; set; }
        public bool isCheck { get; set; }
    }

    public class UsuariosAuthorizaLista
    {
        public int total { get; set; }
        public List<UsuariosAutorizaData> lista { get; set; } = new List<UsuariosAutorizaData>();
    }

    public class RangosDto
    {
        public string cod_rango { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public int monto_minimo { get; set; }
        public int monto_maximo { get; set; }
        public string? registro_usuario { get; set; }
        public DateTime? registro_fecha { get; set; }
        public string? modifica_usuario { get; set; }
        public DateTime? modifica_fecha { get; set; }
    }

    public class RangosUsuariosDto
    {
        public int cod_rango_usuario { get; set; }
        public string cod_rango { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public string nombre_usuario { get; set; } = string.Empty;
        public bool activo  { get; set; }
        public string uen { get; set; } = string.Empty;
        public string nombre_uen { get; set; } = string.Empty;
        public string registro_usuario { get; set; } = string.Empty;
    }
}