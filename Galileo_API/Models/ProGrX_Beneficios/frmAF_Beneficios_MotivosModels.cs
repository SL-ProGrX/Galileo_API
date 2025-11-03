namespace PgxAPI.Models.AF
{
    public class BENE_MOTIVOS
    {
        public string cod_motivo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool activo { get; set; }
        public DateTime? registro_fecha { get; set; }
        public string? registro_usuario { get; set; }
        public DateTime? modifica_fecha { get; set; }
        public string? modifica_usuario { get; set; }

    }

    public class BENE_MOTIVOSDataLista
    {
        public int Total { get; set; }
        public List<BENE_MOTIVOS> Lista { get; set; } = new List<BENE_MOTIVOS>();
    }

}