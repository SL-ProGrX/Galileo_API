namespace Galileo.Models.AF
{
    public class AfiBenePagoData
    {
        public string item { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }
    public class AfiBenePago
    {
        public string cedula { get; set; } = string.Empty;
        public int consec { get; set; }
        public string cod_beneficio { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public float monto { get; set; }
        public int cod_banco { get; set; }
        public string tipo_emision { get; set; } = string.Empty;
        public string cta_bancaria { get; set; } = string.Empty;
        public string? tesoreria { get; set; }
        public string estado { get; set; } = string.Empty;
        public string? envio_user { get; set; }
        public DateTime? envio_fecha { get; set; }
        public string? tes_supervision_usuario { get; set; }
        public DateTime? tes_supervision_fecha { get; set; }
        public string? id_token { get; set; }
        public string? nombre { get; set; }
        public string? nombreBanco { get; set; }
        public bool? select { get; set; }
    }

}
