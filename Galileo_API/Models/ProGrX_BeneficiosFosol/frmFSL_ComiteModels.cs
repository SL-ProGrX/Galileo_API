namespace PgxAPI.Models.FSL
{
    public class FslComitesDTO
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
        public List<FslComitesDTO> Comites { get; set; } = new List<FslComitesDTO>();
    }

    public class FslMiembrosComitesDTO
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
        public List<FslMiembrosComitesDTO> Miembros { get; set; } = new List<FslMiembrosComitesDTO>();
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
