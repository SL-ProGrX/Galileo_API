namespace Galileo_API.Models.ProGrX_Contabilidad
{
    public class CntXConciliacionMovRequest
    {
        public int cod_contabilidad { get; set; } = 0;
        public string usuario { get; set; } = string.Empty;
        public string cuenta { get; set; } = string.Empty;
        public DateTime fecha_inicio { get; set; } = DateTime.Now;
        public DateTime fecha_corte { get; set; } = DateTime.Now;
    }

    public class CntXConciliacionResult
    {
        public List<CntXConciliacionMovData> debitos { get; set; } = new List<CntXConciliacionMovData>();
        public List<CntXConciliacionMovData> creditos { get; set; } = new List<CntXConciliacionMovData>();
        public List<CntXConciliacionMovData> conciliados { get; set; } = new List<CntXConciliacionMovData>();
    }

    public class CntXConciliacionProcesoData
    {
        public int pendientes { get; set; } = 0;
        public int total { get; set; } = 0;
    }

    public class CntXConciliacionMovData
    {
        public int num_linea { get; set; } = 0;
        public string tipo_asiento { get; set; } = string.Empty;
        public string num_asiento { get; set; } = string.Empty;
        public decimal monto { get; set; } = 0;
        public decimal tipo_cambio { get; set; } = 0;
        public string documento { get; set; } = string.Empty;
        public string detalle { get; set; } = string.Empty;
        public DateTime? fecha { get; set; }
        public string referencia { get; set; } = string.Empty;

        public string cr_tipo_asiento { get; set; } = string.Empty;
        public string cr_num_asiento { get; set; } = string.Empty;
        public decimal cr_monto { get; set; } = 0;
        public decimal cr_tipo_cambio { get; set; } = 0;
        public string cr_documento { get; set; } = string.Empty;
        public string cr_detalle { get; set; } = string.Empty;
        public DateTime? cr_fecha { get; set; }
        public string cr_referencia { get; set; } = string.Empty;

    }
}
