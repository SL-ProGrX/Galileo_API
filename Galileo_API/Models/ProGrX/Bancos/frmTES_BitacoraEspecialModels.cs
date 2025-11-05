namespace PgxAPI.Models.TES
{
    public class FiltrosBitacoraEspecial
    {
        public List<DropDownListaGenericaModel<int>>? cuentas { get; set; }
        public List<DropDownListaGenericaModel<string>>? tipos_documento { get; set; }
        public string? estado { get; set; }
        public string? tipo_fecha { get; set; }
        public DateTime fecha_inicio { get; set; }
        public DateTime fecha_corte { get; set; }
        public List<DropDownListaGenericaModel<string>>? movimientos { get; set; }
        public DateTime mov_fecha_inicio { get; set; }
        public DateTime mov_fecha_corte { get; set; }
        public bool chk_revision { get; set; }
        public string? usuario { get; set; }
        public string? revision { get; set; }
    }

    public class BitacoraEspecialDto
    {
        public int nsolicitud { get; set; }
        public string ndocumento { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public string estado { get; set; } = string.Empty;
        public DateTime fecha { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public string detalle { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public string revisado_usuario { get; set; } = string.Empty;
        public DateTime? revisado_fecha { get; set; }
        public int id { get; set; }
        public bool revisado { get; set; }
    }
}