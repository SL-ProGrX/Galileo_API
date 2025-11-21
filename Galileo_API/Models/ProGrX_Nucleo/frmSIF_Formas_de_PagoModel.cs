namespace Galileo.Models.ProGrX_Nucleo
{
    public class SifFormasPago
    {
        public string? cod_forma_pago { get; set; }
        public string? descripcion { get; set; }
        public bool activa { get; set; }
        public bool efectivo { get; set; }
        public string? tipo { get; set; }
        public string? cod_cuenta { get; set; }
        public bool aplica_saldos_favor { get; set; }
        public bool aplica_para_deposito { get; set; }
        public DateTime registro_fecha { get; set; }
        public string? registro_usuario { get; set; }
        public int maximo_apl { get; set; }
        public decimal maximo_monto { get; set; }
        public bool or_aplica { get; set; }
        public bool or_diario_apl { get; set; }
        public decimal or_diario_monto { get; set; }
        public bool or_mensual_apl { get; set; }
        public decimal or_mensual_monto { get; set; }
        public string? codigo_fe { get; set; }
        public bool recibo_digital { get; set; }
        public string? tipo_desc { get; set; }
        public string? cuenta_mask { get; set; }
        public string? cuenta_desc { get; set; }
    }

    public class SifFormasPagoList
    {
        public string? cod_forma_pago { get; set; }
        public string? descripcion { get; set; }
    }

}