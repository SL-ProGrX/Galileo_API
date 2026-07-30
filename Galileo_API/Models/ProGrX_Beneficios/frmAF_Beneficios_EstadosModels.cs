namespace Galileo.Models.AF
{
    public class BeneEstado
    {
        public string cod_estado { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        [System.Text.Json.Serialization.JsonRequired]
        public bool activo { get; set; }
        public string orden { get; set; } = string.Empty;
        [System.Text.Json.Serialization.JsonRequired]
        public bool p_inicia { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public bool p_finaliza { get; set; }
        public DateTime? registro_fecha { get; set; }
        public string? registro_usuario { get; set; }
        public DateTime? modifica_fecha { get; set; }
        public string? modifica_usuario { get; set; }
        public string proceso { get; set; } = string.Empty;

        /// <summary>Indica que la fila es nueva en la tabla de Angular; no persiste en base de datos.</summary>
        public bool isNew { get; set; }
    }

    public class BeneEstadoDataLista
    {
        public int total { get; set; }
        public List<BeneEstado> lista { get; set; } = new List<BeneEstado>();
    }
}
