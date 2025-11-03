namespace PgxAPI.Models.AF
{
    public class BENE_ESTADO
    {
        public string cod_estado { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool activo { get; set; }
        public string orden { get; set; } = string.Empty;
        public bool p_inicia { get; set; }
        public bool p_finaliza { get; set; }
        public DateTime? registro_fecha { get; set; }
        public string? registro_usuario { get; set; }
        public DateTime? modifica_fecha { get; set; }
        public string? modifica_usuario { get; set; }
        public string proceso { get; set; } = string.Empty;

    }

    public class BENE_ESTADODataLista
    {
        public int Total { get; set; }
        public List<BENE_ESTADO> Lista { get; set; } = new List<BENE_ESTADO>();
    }
}
