namespace Galileo_API.Models.ProGrX_Contabilidad
{
    
        public class CntxMovimientosFiltroDto
        {
            public string? tipo { get; set; }
            public string? num_asiento { get; set; }
            public string? unidad { get; set; }
            public string? cc { get; set; }
            public string? divisa { get; set; }
            public string? documento { get; set; }
            public string? detalle { get; set; }
            public string? referencia { get; set; }
            public string? cuenta { get; set; }

            public DateTime? fecha_desde { get; set; }
            public DateTime? fecha_hasta { get; set; }

            public bool todas { get; set; }

            public int lineas { get; set; } = 1000;
    }

    public class CntxMovimientoConsultaDto
    {
        public string? tipo_asiento { get; set; }
        public string? num_asiento { get; set; }
        public DateTime fecha_asiento { get; set; }

        public string? cod_cuenta { get; set; }

        public string? cod_unidad { get; set; }
        public string? cod_centro_costo { get; set; }

        public string? cod_divisa { get; set; }
        public decimal? tipo_cambio { get; set; }

        public string? documento { get; set; }
        public string? detalle { get; set; }

        public decimal? monto_credito { get; set; }
        public decimal? monto_debito { get; set; }
    }
}
