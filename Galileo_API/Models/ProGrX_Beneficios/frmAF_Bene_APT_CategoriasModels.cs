
namespace Galileo.Models.AF
{
    public class AptCategorias
    {
        public int id_apt_categoria { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public bool activo { get; set; }
        public DateTime? registro_fecha { get; set; }
        public string? registro_usuario { get; set; }
        public DateTime? modifica_fecha { get; set; }
        public string? modifica_usuario { get; set; }
    }

    public class AptCategoriasDataLista
    {
        public int Total { get; set; }
        public List<AptCategorias> Lista { get; set; } = new List<AptCategorias>();
    }
}