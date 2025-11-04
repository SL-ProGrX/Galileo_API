namespace PgxAPI.Models
{
    public class CargoPeriodicoDto
    {
        public string Cod_Cargo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }

    public class TipoOrdenDto
    {
        public string Tipo_Orden { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }

    public class UnidadesDtoList
    {
        public int Total { get; set; }
        public List<UnidadesDto> Unidades { get; set; } = new List<UnidadesDto>();
    }

    public class UnidadesDto
    {
        public string unidad { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class CentroCostoDtoList
    {
        public int Total { get; set; }
        public List<CentroCostoDto> CentroCostos { get; set; } = new List<CentroCostoDto>();
    }

    public class CentroCostoDto
    {
        public string centrocosto { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class MComprasFiltros
    {
        public int CodConta { get; set; }
        public int? pagina { get; set; }
        public int? paginacion { get; set; }
        public string? filtro { get; set; }
    }

    public class CatalogoDto
    {
        public string item { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }
}
