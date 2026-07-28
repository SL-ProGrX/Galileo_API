namespace Galileo.Models.AF
{
    public class BeneRequisitosDataLista
    {
        public int total { get; set; }
        public List<BeneRequisitosData> lista { get; set; } = new List<BeneRequisitosData>();
    }

    public class BeneRequisitosData
    {
        public string cod_requisito { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        [System.Text.Json.Serialization.JsonRequired]
        public bool activo { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public bool requerido { get; set; }
        public string? registro_usuario { get; set; }
    }

    public class AfiRequerimientoFiltros
    {
        public int? pagina { get; set; }
        public int? paginacion { get; set; }
        public string? filtro { get; set; }
    }
}
