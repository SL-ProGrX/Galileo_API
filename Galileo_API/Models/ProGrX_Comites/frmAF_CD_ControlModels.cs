namespace Galileo_API.Models.ProGrX_Comites
{
    public class AFCDCuentaDto
    {
        public int noperacion { get; set; }
        public string? comite_desc { get; set; }
        public decimal monto { get; set; }
        public string? estado_desc { get; set; }
        public string? proceso_desc { get; set; }
        public DateTime? registro_fecha { get; set; }
        public string? registro_usuario { get; set; }
    }

    public class AFCDCuentaFiltroDto
    {
        public string? comite { get; set; }
        public string? tipo { get; set; }
        public string? estado { get; set; }
        public string? proceso { get; set; }
        public DateTime? fecha_inicio { get; set; }
        public DateTime? fecha_fin { get; set; }
        public bool todas { get; set; }
        public int tesoreria_id { get; set; }
    }


}
