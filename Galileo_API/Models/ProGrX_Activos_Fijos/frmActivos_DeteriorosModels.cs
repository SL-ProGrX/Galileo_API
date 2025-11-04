namespace PgxAPI.Models.ProGrX_Activos_Fijos
{
    public class ActivosDeterioroData
    {
        public int id_addret { get; set; }
        public string num_placa { get; set; } = string.Empty;
        public DateTime fecha { get; set; }
        public decimal monto { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public string motivo_id { get; set; } = string.Empty;
        public string motivo_desc { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string cod_proveedor { get; set; } = string.Empty;
        public string proveedor { get; set; } = string.Empty;
        public string creacion_user { get; set; } = string.Empty;
        public DateTime creacion_fecha { get; set; }
        public decimal depreciacion_mes { get; set; }
        public decimal depreciacion_acum { get; set; }
        public DateTime depreciacion_periodo { get; set; } 
        public decimal valor_libros { get; set; }
    }

    public class ActivosDeterioroDetallaData
    {
        public string num_placa { get; set; } = string.Empty;
        public string depreciacionPeriodo { get; set; } = string.Empty;
        public decimal depreciacion_acum { get; set; }
        public decimal valor_historico { get; set; }
        public decimal valor_desecho { get; set; }
        public decimal valor_libros { get; set; }
        public DateTime depreciacion_periodo { get; set; }
    }
}