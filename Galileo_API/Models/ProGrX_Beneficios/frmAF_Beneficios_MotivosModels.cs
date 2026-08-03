namespace Galileo.Models.AF
{
    public class BeneMotivos
    {
        [System.Text.Json.Serialization.JsonRequired]
        public string cod_motivo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        [System.Text.Json.Serialization.JsonRequired]
        public bool activo { get; set; }
        public DateTime? registro_fecha { get; set; }
        public string? registro_usuario { get; set; }
        public DateTime? modifica_fecha { get; set; }
        public string? modifica_usuario { get; set; }

        /// <summary>Indica que la fila es nueva en la tabla de Angular; no persiste en base de datos.</summary>
        [System.Text.Json.Serialization.JsonRequired]
        public bool isNew { get; set; }
    }

    public class BeneMotivosDataLista
    {
        public int total { get; set; }
        public List<BeneMotivos> lista { get; set; } = new List<BeneMotivos>();
    }
}
