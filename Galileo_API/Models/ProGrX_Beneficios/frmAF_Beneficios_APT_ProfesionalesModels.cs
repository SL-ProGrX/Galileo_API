namespace Galileo.Models.AF
{
    public class BeneAptProfesionalesDataLista
    {
        public int total { get; set; }
        public List<BeneAptProfesionalesData> lista { get; set; } = new List<BeneAptProfesionalesData>();
    }

    public class BeneAptProfesionalesData
    {
        [System.Text.Json.Serialization.JsonRequired]
        public long id_profesional { get; set; }
        public string identificacion { get; set; } = string.Empty;
        public string? nombre { get; set; }
        public string? usuario { get; set; }
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
}
