namespace Galileo_API.Models.ProGrX.Cobros
{
    public class CoGruposData
    {
        public int id_grupo { get; set; } = 0;
        public string descripcion { get; set; } = string.Empty;
        public bool activo { get; set; } = false;
        public string usuario { get; set; } = string.Empty;
    }

    public class CoGruposAsignacionFiltros
    {
        public int id_grupo { get; set; } = 0;
        public string? filtro { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
    }

    public class CoGruposAsignacionData
    {
        public string idX { get; set; } = string.Empty;
        public string itmX { get; set; } = string.Empty;
        public string? registro_usuario { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
        public bool? asignado { get; set; } 
    }
}
