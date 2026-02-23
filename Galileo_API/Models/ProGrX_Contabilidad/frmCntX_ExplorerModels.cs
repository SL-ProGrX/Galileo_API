namespace Galileo_API.Models.ProGrX_Contabilidad
{
    using System;

    namespace Galileo_API.Models.ProGrX_Contabilidad
    {
        public class CntxExploradorFiltrosDto
        {

            public string? tipo { get; set; }
            public string? unidad { get; set; }
            public string? cc { get; set; }

            public string? mov_tipo { get; set; } = "NA";
            public decimal mov_desde { get; set; } = 0;
            public decimal mov_hasta { get; set; } = 999999999999;

            public string? num_asiento { get; set; }
            public string? num_documento { get; set; }
            public string? detalle { get; set; }
            public string? referencia { get; set; }

            public DateTime? fecha_desde { get; set; }
            public DateTime? fecha_hasta { get; set; }
            public bool? todas { get; set; }

            public string? divisa { get; set; }
            public string? cuenta_inicio { get; set; }
            public string? cuenta_corte { get; set; }

            public int? lineas { get; set; } = 1000;


            public string? cod_cuenta { get; set; }
            public string? cod_tipo_asiento { get; set; }
            public string? cod_periodo { get; set; }
        }

        public class CntxAsientoRsmDto
        {
            public string? num_asiento { get; set; }
            public string? tipo_asiento { get; set; }
            public DateTime? fecha_asiento { get; set; }
            public string? descripcion { get; set; }

            public decimal debe { get; set; }
            public decimal haber { get; set; }

            public string? aplicado { get; set; } 
            public string? referencia { get; set; }
        }

        public class CntxAsientoDetDto
        {
            public string? cod_cuenta_mask { get; set; }
            public string? cod_cuenta { get; set; }
            public string? cuenta_desc { get; set; }

            public decimal monto_debito { get; set; }
            public decimal monto_credito { get; set; }

            public string? documento { get; set; }
            public string? detalle { get; set; }

            public string? cod_unidad { get; set; }
            public string? cod_centro_costo { get; set; }
            public string? cod_divisa { get; set; }

            public decimal tipo_cambio { get; set; }
            public decimal importe { get; set; }
        }

        public class CntxPeriodoDto
        {
            public int anio { get; set; }
            public int mes { get; set; }
            public DateTime? periodo_corte { get; set; }
            public string? estado { get; set; } 
            public string? cierre_usuario { get; set; }
            public DateTime? cierre_fecha { get; set; }

            public string periodo => $"{anio}-{mes:00}";
            public string fecha_periodo => $"{anio}-{mes:00}";
        }
    }
}
