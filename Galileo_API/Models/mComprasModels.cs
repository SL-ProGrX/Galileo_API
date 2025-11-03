namespace PgxAPI.Models
{

    public class CargoPeriodicoDTO
    {
        public string Cod_Cargo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }

    public class TipoOrdenDTO
    {
        public string Tipo_Orden { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }

    public class UnidadesDTOList
    {
        public int Total { get; set; }
        public List<UnidadesDTO> unidades { get; set; } = new List<UnidadesDTO>();
    }

    public class UnidadesDTO
    {
        public string unidad { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class CentroCostoDTOList
    {
        public int Total { get; set; }
        public List<CentroCostoDTO> centrocostos { get; set; } = new List<CentroCostoDTO>();
    }

    public class CentroCostoDTO
    {
        public string centrocosto { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class mComprasFiltros
    {
        public int CodConta { get; set; }
        public int? pagina { get; set; }
        public int? paginacion { get; set; }
        public string? filtro { get; set; }
    }

    public class CatalogoDTO
    {
        public string item { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }
}
