namespace Galileo.Models.AF
{
    public class BeneMotivos
    {
        public string cod_motivo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool activo { get; set; }
        public DateTime? registro_fecha { get; set; }
        public string? registro_usuario { get; set; }
        public DateTime? modifica_fecha { get; set; }
        public string? modifica_usuario { get; set; }

    }

    public class BeneMotivosDataLista
    {
        public int Total { get; set; }
        public List<BeneMotivos> Lista { get; set; } = new List<BeneMotivos>();
    }
}