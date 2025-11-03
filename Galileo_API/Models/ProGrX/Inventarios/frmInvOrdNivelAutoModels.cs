namespace PgxAPI.Models.INV
{
    public class AutorizadorDTO
    {
        public string Usuario { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public DateTime? Fecha { get; set; }
        public string Estado { get; set; } = string.Empty;
        public DateTime? Ult_Modificacion { get; set; }
    }

    public class AutorizadorDataLista
    {
        public int Total { get; set; }
        public List<AutorizadorDTO> Autorizadores { get; set; } = new List<AutorizadorDTO>();
    }

    public class UsuarioaCargoDTO
    {
        public string Usuario { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Entradas { get; set; }
        public bool Salidas { get; set; }
        public bool Requisiciones { get; set; }
        public bool Traslados { get; set; }
        public string Autorizador { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
    }

    public class UsuariosACargoDataLista
    {
        public int Total { get; set; }
        public List<UsuarioaCargoDTO> Usuarios { get; set; } = new List<UsuarioaCargoDTO>();
    }

    public class UsuarioaCambioFechaDTO
    {
        public string Usuario { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
    }

    public class UsuariosCambioFchDataLista
    {
        public int Total { get; set; }
        public List<UsuarioaCambioFechaDTO> Usuarios { get; set; } = new List<UsuarioaCambioFechaDTO>();
    }

}
