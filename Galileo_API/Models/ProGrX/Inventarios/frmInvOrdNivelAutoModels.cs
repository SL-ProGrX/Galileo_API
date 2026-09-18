namespace Galileo.Models.INV
{
    public class AutorizadorDto
    {
        public string usuario { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public DateTime? fecha { get; set; }
        public string estado { get; set; } = string.Empty;
        public DateTime? ult_modificacion { get; set; }
    }

    public class AutorizadorDataLista
    {
        public int total { get; set; } = 0;
        public List<AutorizadorDto> autorizadores { get; set; } = new();
    }

    public class UsuarioaCargoDto
    {
        public string usuario { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool entradas { get; set; } = false;
        public bool salidas { get; set; } = false;
        public bool requisiciones { get; set; } = false;
        public bool traslados { get; set; } = false;
        public string autorizador { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
    }

    public class UsuariosACargoDataLista
    {
        public int total { get; set; } = 0;
        public List<UsuarioaCargoDto> usuarios { get; set; } = new();
    }

    public class UsuarioaCambioFechaDto
    {
        public string usuario { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
    }

    public class UsuariosCambioFchDataLista
    {
        public int total { get; set; } = 0;
        public List<UsuarioaCambioFechaDto> usuarios { get; set; } = new();
    }
}