using Galileo.Models;

namespace Galileo_API.Models.ProGrX.Credito
{
    public class CrArregloPagoCajaInicialData
    {
        public List<DropDownListaGenericaModel> tipos_documento { get; set; } = new();
        public DateTime? fecha_servidor { get; set; }
        public bool sys_plan_pagos { get; set; } = false;
    }

    public class CrArregloPagoOperacionData
    {
        public int operacion { get; set; } = 0;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string codigo { get; set; } = string.Empty;
        public string linea_desc { get; set; } = string.Empty;
        public string proceso { get; set; } = string.Empty;
        public int opex { get; set; } = 0;
        public bool retencion { get; set; } = false;
        public decimal monto { get; set; } = 0;
        public decimal saldo { get; set; } = 0;
        public int plazo { get; set; } = 0;
        public decimal tasa { get; set; } = 0;
        public decimal cuota { get; set; } = 0;
        public string divisa { get; set; } = "COL";
        public DateTime fecha_servidor { get; set; }
        public DateTime? fecha_ult_mov { get; set; }
        public bool sys_plan_pagos { get; set; } = false;
        public int mora_count { get; set; } = 0;

        public decimal int_cor { get; set; } = 0;
        public decimal int_mor { get; set; } = 0;
        public decimal cargos { get; set; } = 0;
        public decimal polizas { get; set; } = 0;
        public decimal amortiza { get; set; } = 0;
        public decimal cargos_intereses { get; set; } = 0;
        public decimal deuda { get; set; } = 0;
        public decimal total_pagar { get; set; } = 0;

        public bool tipo_intereses { get; set; } = false;
        public long prideduc { get; set; } = 0;

        public List<CrArregloPagoMoraData> mora { get; set; } = new();
    }

    public class CrArregloPagoPeriodoGraciaRequest
    {
        public string usuario { get; set; } = string.Empty;
        public int operacion { get; set; } = 0;
        public string notas { get; set; } = string.Empty;
        public string tipo_aplicacion { get; set; } = "TOTAL";
        public DateTime? fecha_inicio { get; set; }
        public DateTime? fecha_corte { get; set; }
        public bool aplica_intereses { get; set; } = false;
        public bool aplica_cargos { get; set; } = false;
        public bool aplica_polizas { get; set; } = false;
        public bool ajusta_plazo { get; set; } = false;
        public bool retroactivo { get; set; } = false;
    }

    public class CrArregloPagoVencimientoInteresesRequest
    {
        public string usuario { get; set; } = string.Empty;
        public int operacion { get; set; } = 0;
        public string notas { get; set; } = string.Empty;
        public DateTime? fecha_corte { get; set; }
    }

    public class CrArregloPagoAplicacionResultadoData
    {
        public string tipo_documento { get; set; } = string.Empty;
        public string num_documento { get; set; } = string.Empty;
        public string tipo_nota { get; set; } = string.Empty;
        public string num_nota { get; set; } = string.Empty;
        public string mensaje { get; set; } = string.Empty;
    }

    public class CrArregloPagoMoraData
    {
        public long id_moro { get; set; } = 0;
        public int id_solicitud { get; set; } = 0;
        public DateTime? fecha_p { get; set; }
        public decimal int_c { get; set; } = 0;
        public decimal int_m { get; set; } = 0;
        public decimal cargo { get; set; } = 0;
        public decimal poliza { get; set; } = 0;
        public decimal amortiza { get; set; } = 0;
        public decimal cuota_morosa { get; set; } = 0;
        public string estado { get; set; } = string.Empty;
        public decimal ab_int_c { get; set; } = 0;
        public decimal ab_int_m { get; set; } = 0;
        public decimal ab_amortiza { get; set; } = 0;
        public decimal ab_cargo { get; set; } = 0;
        public decimal ab_poliza { get; set; } = 0;
    }

    public class CrArregloPagoCajaRequestBase
    {
        public string usuario { get; set; } = string.Empty;
        public string caja { get; set; } = string.Empty;
        public int apertura { get; set; } = 0;
        public string tiquete { get; set; } = string.Empty;
        public string unidad { get; set; } = string.Empty;
        public string divisa { get; set; } = string.Empty;
        public int operacion { get; set; } = 0;
        public string tipo_doc { get; set; } = string.Empty;
        public decimal total_cajas { get; set; } = 0;
        public string notas { get; set; } = string.Empty;
    }

    public class CrArregloPagoCapitalizaRequest : CrArregloPagoCajaRequestBase
    {
        public bool trasladar { get; set; } = false;
        public bool tipo_intereses { get; set; } = false;
    }

    public class CrArregloPagoAbonoEspecialRequest : CrArregloPagoCajaRequestBase
    {
        public string tipo_abono { get; set; } = "E";
        public string proceso_cuota { get; set; } = string.Empty;
        public int num_cuota { get; set; } = 0;
        public decimal int_cor { get; set; } = 0;
        public decimal int_mor { get; set; } = 0;
        public decimal principal { get; set; } = 0;
        public decimal polizas { get; set; } = 0;
        public decimal cargos { get; set; } = 0;
    }
}