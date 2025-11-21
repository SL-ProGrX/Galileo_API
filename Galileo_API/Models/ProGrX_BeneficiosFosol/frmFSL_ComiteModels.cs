namespace Galileo.Models.FSL
{
    public class FslComitesDto
    {
        public string cod_comite { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public int numero_resolutores { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public bool activo { get; set; }
    }

    public class FslComitesDataLista
    {
        public int Total { get; set; }
        public List<FslComitesDto> Comites { get; set; } = new List<FslComitesDto>();
    }

    public class FslMiembrosComitesDto
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string usuario_Vinculado { get; set; } = string.Empty;
        public string cod_comite { get; set; } = string.Empty;
        public DateTime registro_Fecha { get; set; }
        public DateTime salida_Fecha { get; set; }
        public string registro_Usuario { get; set; } = string.Empty;
        public string salida_usuario { get; set; } = string.Empty;
        public bool activo { get; set; }
    }

    public class FslMiembrosComitesDataLista
    {
        public int Total { get; set; }
        public List<FslMiembrosComitesDto> Miembros { get; set; } = new List<FslMiembrosComitesDto>();
    }

    public class FslComitesActivosData()
    {
        public string item { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class FslComitefiltros
    {
        public int? pagina { get; set; }
        public int? paginacion { get; set; }
        public string? filtro { get; set; }

        public string? comiteSeleccionado { get; set; }
    }
}