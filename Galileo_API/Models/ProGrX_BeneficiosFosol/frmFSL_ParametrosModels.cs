namespace Galileo.Models.FSL
{
    public class FdlParametrosDto
    {
        public string cod_parametro { get; set; } = string.Empty;
        public string detalle { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string valor { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public DateTime registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
    }

    public class FdlParametrosFiltros
    {
        public int? pagina { get; set; }
        public int? paginacion { get; set; }
        public string? filtro { get; set; }

        public string? comiteSeleccionado { get; set; }
    }

    public class FdlParametrosListaDto
    {
        public int Total { get; set; }
        public List<FdlParametrosDto> Comites { get; set; } = new List<FdlParametrosDto>();
    }
}