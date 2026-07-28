namespace Galileo.Models.AF
{
    public class BeneRegRequisito
    {
        public string cod_requisito { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public DateTime registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public bool asigna { get; set; }
    }

    public class BeneRequisitosGuardar
    {
        [System.Text.Json.Serialization.JsonRequired]
        public int codCliente { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public int consec { get; set; }
        public string cod_requisito { get; set; } = string.Empty;
        public string cod_beneficio { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
    }

    public class BeneRegRequisitoLista
    {
        public int total { get; set; }
        public List<BeneRegRequisito> lista { get; set; } = new List<BeneRegRequisito>();
    }
}
