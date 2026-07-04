namespace Galileo_API.Models.ProGrX.Credito
{
    public class CrPrendasExtrasEncabezadoData
    {
        public long prenda_id { get; set; }
        public string tipo_prenda_desc { get; set; } = string.Empty;
        public long id_solicitud { get; set; }
        public string cod_preanalisis { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
    }

    public class CrPrendasExtrasData
    {
        public long id_extra { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public decimal monto_extras { get; set; }
    }

    public class CrPrendasExtrasConsultaData
    {
        public CrPrendasExtrasEncabezadoData encabezado { get; set; } = new();
        public List<CrPrendasExtrasData> extras { get; set; } = new();
        public decimal total_monto { get; set; }
    }

    public class CrPrendasExtrasGuardarRequest
    {
        public long prenda_id { get; set; }
        public string usuario { get; set; } = string.Empty;
        public List<CrPrendasExtrasData> extras { get; set; } = new();
    }

    public class CrPrendasExtrasGuardarData
    {
        public decimal total_monto { get; set; }
    }

    public class CrPrendasExtrasSpResult
    {
        public int pass { get; set; }
        public string movimiento { get; set; } = string.Empty;
        public string mensaje { get; set; } = string.Empty;
    }
}
