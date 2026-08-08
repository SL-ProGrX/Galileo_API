namespace Galileo.Models.AF
{
    public class BeneRequisitosDataLista
    {
        public int total { get; set; }
        public List<BeneRequisitosData> lista { get; set; } = new List<BeneRequisitosData>();
    }

    public class BeneRequisitosData
    {
        [System.Text.Json.Serialization.JsonRequired]
        public string cod_requisito { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        [System.Text.Json.Serialization.JsonRequired]
        public bool activo { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public bool requerido { get; set; }
        public string? registro_usuario { get; set; }

        /// <summary>Indica que la fila es nueva en la tabla de Angular; no persiste en base de datos.</summary>
        [System.Text.Json.Serialization.JsonRequired]
        public bool isNew { get; set; }
    }
}
