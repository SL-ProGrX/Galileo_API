namespace Galileo_API.Models.ProGrX_EstudioCrd
{
    namespace Galileo_API.Models.ProGrX_EstudioCrd
    {
        public class FrmPreaFechaFormalizaCargarRequest
        {
            public string usuario { get; set; } = string.Empty;
            public string cod_preanalisis { get; set; } = string.Empty;
        }

        public class FrmPreaFechaFormalizaCargarResponse
        {
            public string cod_preanalisis { get; set; } = string.Empty;
            public DateTime? planilla_aplica { get; set; }
            public DateTime? planilla_envio { get; set; }
            public DateTime? fecha_corte { get; set; }
            public DateTime? formalizacion { get; set; }
            public decimal monto { get; set; }
            public decimal tasa { get; set; }
            public int dias { get; set; }
            public decimal monto_interes { get; set; }
        }

        public class FrmPreaFechaFormalizaCalcularRequest
        {
            public string usuario { get; set; } = string.Empty;
            public string cod_preanalisis { get; set; } = string.Empty;
            public DateTime? fecha_formaliza { get; set; }
        }

        public class FrmPreaFechaFormalizaCalcularResponse
        {
            public DateTime? fecha_corte { get; set; }
            public DateTime? fecha_formaliza { get; set; }
            public int dias { get; set; }
            public decimal monto_interes { get; set; }
        }

        public class FrmPreaFechaFormalizaCambiarRequest
        {
            public string usuario { get; set; } = string.Empty;
            public string cod_preanalisis { get; set; } = string.Empty;
            public DateTime? fecha_formaliza { get; set; }
            public decimal monto_interes { get; set; } = 0;
        }

        public class FrmPreaFechaFormalizaCambiarResponse
        {
            public DateTime? fecha_formaliza { get; set; }
            public int dias { get; set; }
            public decimal monto_interes { get; set; }
            public string mensaje { get; set; } = string.Empty;
        }

        internal class FrmPreaFechaFormalizaBaseData
        {
            public DateTime? planilla_aplica { get; set; }
            public DateTime? planilla_envio { get; set; }
            public DateTime? fecha_corte { get; set; }
            public DateTime? formalizacion { get; set; }
            public decimal monto { get; set; }
            public decimal tasa { get; set; }
        }

        internal class FrmPreaFechaFormalizaCalculoSpData
        {
            public int dias { get; set; }
            public decimal monto_interes { get; set; }
        }
    }

}
