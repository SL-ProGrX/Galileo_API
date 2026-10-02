namespace Galileo.Models.FSL
{
    public class FslParametroDto
    {
        public string cod_parametro { get; set; } = string.Empty;
        public string detalle { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string valor { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public string valor_descripcion { get; set; } = string.Empty;
    }

    public class FslParametrosFiltros
    {
        public int pagina { get; set; } = 0;
        public int paginacion { get; set; } = 30;
        public string filtro { get; set; } = string.Empty;
        public string sort_field { get; set; } = "cod_parametro";
        public int sort_order { get; set; } = 1;
    }

    public class FslParametrosListaDto
    {
        public int total { get; set; } = 0;
        public List<FslParametroDto> parametros { get; set; } =
            new List<FslParametroDto>();
    }

    public class FslParametroActualizarRequest
    {
        public string cod_parametro { get; set; } = string.Empty;
        public string valor { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
    }
}