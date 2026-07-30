
namespace Galileo.Models.AF
{
    public class AptCategorias
    {
        [System.Text.Json.Serialization.JsonRequired]
        public int id_apt_categoria { get; set; }
        public string descripcion { get; set; } = string.Empty;
        [System.Text.Json.Serialization.JsonRequired]
        public bool activo { get; set; }
        public DateTime? registro_fecha { get; set; }
        public string? registro_usuario { get; set; }
        public DateTime? modifica_fecha { get; set; }
        public string? modifica_usuario { get; set; }

        /// <summary>Indica que la fila es nueva en la tabla de Angular; no persiste en base de datos.</summary>
        public bool isNew { get; set; }
    }

    public class AptCategoriasDataLista
    {
        public int total { get; set; }
        public List<AptCategorias> lista { get; set; } = new List<AptCategorias>();
    }
}
