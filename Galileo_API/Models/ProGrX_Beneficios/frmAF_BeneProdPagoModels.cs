namespace Galileo.Models.AF
{
    public class AfiBeneProdAsgDataList
    {
        public int total { get; set; } = 0;
        public List<AfiBeneProdAsgData> beneficios { get; set; } = new();
    }

    public class AfiBeneProdAsgData
    {
        public int consec { get; set; } = 0;
        public string cod_beneficio { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public decimal cantidad { get; set; } = 0;
        public decimal monto { get; set; } = 0;
    }

    public class AfiBeneProdDetalleData
    {
        public int consec { get; set; } = 0;
        public string cod_beneficio { get; set; } = string.Empty;
        public string cod_producto { get; set; } = string.Empty;
        public string producto_desc { get; set; } = string.Empty;
        public decimal cantidad { get; set; } = 0;
        public decimal costo_unidad { get; set; } = 0;
    }

    public class AfiBeneProdPagoListaRequest
    {
        public string cod_beneficio { get; set; } = string.Empty;
        public int pagina { get; set; } = 0;
        public int paginacion { get; set; } = 30;
        public string filtro { get; set; } = string.Empty;
    }

    public class AfiBeneProdPagoEntregaRequest
    {
        public string usuario { get; set; } = string.Empty;
        public List<AfiBeneProdAsgData> registros { get; set; } = new();
    }
}