namespace Galileo_API.Models.ProGrX_EstudioCrd
{
    public class FrmPreAnalisisOperacionData
    {
        public long id_solicitud { get; set; }
        public decimal monto_solicitado { get; set; }
        public string codigo { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public decimal cuota { get; set; }
        public string nombre { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
    }

    public class FrmPreAnalisisObligacionData
    {
        public long id_solicitud { get; set; }
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public decimal saldo { get; set; }
        public decimal amortiza { get; set; }
        public decimal int_c { get; set; }
        public decimal int_m { get; set; }
        public decimal cuota { get; set; }
        public string tipo { get; set; } = string.Empty;
        public bool seleccionada { get; set; }
    }

    public class FrmPreAnalisisRegistroData
    {
        public decimal salario_liquido { get; set; }
        public decimal obligaciones_ext_saldo { get; set; }
        public decimal obligaciones_ext_cuota { get; set; }
        public decimal porc_salario_devengado { get; set; }
        public decimal porc_liquidez { get; set; }
        public string observacion_analista { get; set; } = string.Empty;
        public string observacion_comite { get; set; } = string.Empty;
        public string observacion_jd { get; set; } = string.Empty;
    }

    public class FrmPreAnalisisFiadorData
    {
        public string cedulaf { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public decimal salario { get; set; }
        public decimal liquidez { get; set; }
        public decimal devengado { get; set; }
        public long? fia_consec { get; set; }
    }

    public class FrmPreAnalisisCargarResponse
    {
        public FrmPreAnalisisOperacionData? operacion { get; set; }
        public List<FrmPreAnalisisObligacionData> obligaciones { get; set; } = [];
        public decimal cuotas_refundidas { get; set; }
        public FrmPreAnalisisRegistroData? registro { get; set; }
        public List<FrmPreAnalisisFiadorData> fiadores { get; set; } = [];
    }

    public class FrmPreAnalisisCalcularRequest
    {
        public required long operacion { get; set; }
        public List<long> seleccionadas { get; set; } = [];
    }

    public class FrmPreAnalisisCalcularResponse
    {
        public decimal obligaciones_internas { get; set; }
        public decimal cuotas_refundidas { get; set; }
        public decimal total_cuota { get; set; }
        public decimal total_saldo { get; set; }
    }

    public class FrmPreAnalisisDeudorGuardarRequest
    {
        public required long operacion { get; set; }
        public string usuario { get; set; } = string.Empty;
        public string codigo { get; set; } = string.Empty;
        public decimal? salario_liquido { get; set; }
        public decimal? obligaciones_ext_saldo { get; set; }
        public decimal? obligaciones_ext_cuota { get; set; }
        public decimal? salario_neto { get; set; }
        public decimal? porc_salario_devengado { get; set; }
        public decimal? porc_liquidez { get; set; }
        public required decimal total_cuota { get; set; }
        public required decimal total_saldo { get; set; }
        public List<FrmPreAnalisisFiadorData> fiadores { get; set; } = [];
    }

    public class FrmPreAnalisisDeudorGuardarResponse
    {
        public bool guardado { get; set; }
        public List<FrmPreAnalisisFiadorData> fiadores { get; set; } = [];
    }

    public class FrmPreAnalisisObservacionesGuardarRequest
    {
        public required long operacion { get; set; }
        public string usuario { get; set; } = string.Empty;
        public string observacion_analista { get; set; } = string.Empty;
        public string observacion_comite { get; set; } = string.Empty;
        public string observacion_jd { get; set; } = string.Empty;
    }

    public class FrmPreAnalisisOperacionLista
    {
        public long operacion { get; set; }
        public string codigo { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }
    public class FrmPreAnalisisComiteData
    {
        public int id_comite { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public int acta { get; set; }
    }
}
