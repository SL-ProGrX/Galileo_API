using Galileo.Models;

namespace Galileo_API.Models.ProGrX.Creditos
{
    public class CrPolizasRegistroOperacionData
    {
        public int operacion { get; set; } = 0;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class CrPolizasRegistroListadoItem
    {
        public int id_solicitud { get; set; } = 0;
        public int num_poliza { get; set; } = 0;
        public string cod_poliza { get; set; } = string.Empty;
        public string poliza_descripcion { get; set; } = string.Empty;
        public string tipo_registro { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public decimal cuota { get; set; } = 0;
        public decimal monto { get; set; } = 0;
    }

    public class CrPolizasRegistroFormData
    {
        public string poliza_operacion { get; set; } = string.Empty;
        public string poliza_linea { get; set; } = string.Empty;
        public int poliza_id { get; set; } = 0;
        public string poliza_contrato { get; set; } = string.Empty;
        public string poliza_estado { get; set; } = string.Empty;
        public string poliza_plan { get; set; } = string.Empty;
        public string poliza_pago_frecuencia { get; set; } = string.Empty;
        public decimal poliza_monto { get; set; } = 0;
        public decimal poliza_cuota { get; set; } = 0;
        public decimal poliza_pago_monto { get; set; } = 0;
        public decimal poliza_cuota_resto_plazo { get; set; } = 0;
        public int poliza_cobertura_meses { get; set; } = 0;
        public decimal recaudado_saldo { get; set; } = 0;
        public int poliza_pagos_num { get; set; } = 0;
        public int poliza_ctas_deduce { get; set; } = 0;
        public bool poliza_plazo_credito { get; set; } = false;
        public DateTime? poliza_fecha_pago { get; set; }
        public DateTime? poliza_cobertura_inicio { get; set; }
        public DateTime? poliza_cobertura_corte { get; set; }
        public string destino { get; set; } = string.Empty;
        public string garantia { get; set; } = string.Empty;
        public string documento { get; set; } = string.Empty;
        public int plazo { get; set; } = 0;
        public decimal monto { get; set; } = 0;
        public string observaciones { get; set; } = string.Empty;
        public int anio { get; set; } = 0;
        public string mes { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string fecha { get; set; } = string.Empty;
        public int plazo_transcurrido { get; set; } = 0;
        public decimal proyectado { get; set; } = 0;
        public decimal pagado { get; set; } = 0;
        public decimal pendiente { get; set; } = 0;
        public List<DropDownListaGenericaModel> destinos { get; set; } = new();
        public List<DropDownListaGenericaModel> garantias { get; set; } = new();
    }

    public class CrPolizasRegistroPagoItem
    {
        public DateTime? fecha { get; set; }
        public decimal monto { get; set; } = 0;
        public decimal saldo { get; set; } = 0;
        public string observacion { get; set; } = string.Empty;
    }

    public class CrPolizasRegistroRecaudacionItem
    {
        public DateTime? fecha { get; set; }
        public decimal monto { get; set; } = 0;
        public decimal saldo { get; set; } = 0;
        public string usuario { get; set; } = string.Empty;
    }

    public class CrPolizasRegistroAcreedorItem
    {
        public string cod_acreedor { get; set; } = string.Empty;
        public string identificacion { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public bool checked_item { get; set; } = false;
    }

    internal sealed class CrPolizasRegistroOperacionBase
    {
        public int id_solicitud { get; set; }
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    internal sealed class CrPolizasRegistroListaBase
    {
        public int id_solicitud { get; set; }
        public int num_poliza { get; set; }
        public string cod_poliza { get; set; } = string.Empty;
        public string poliza_descripcion { get; set; } = string.Empty;
        public string integra_plan_pagos { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public decimal cuota { get; set; }
        public decimal monto { get; set; }
        public int id_solicitud_poliza { get; set; }
    }

    internal sealed class CrPolizasRegistroDetalleBase
    {
        public string cod_poliza { get; set; } = string.Empty;
        public string poliza_descripcion { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string num_contrato { get; set; } = string.Empty;
        public decimal monto { get; set; } = 0;
        public decimal cuota { get; set; } = 0;
        public decimal pago_monto { get; set; } = 0;
        public decimal cuota_rst_plan { get; set; } = 0;
        public int deduce_plazo_credito { get; set; } = 0;
        public DateTime? pago_fecha { get; set; }
        public DateTime? cobertura_inicio { get; set; }
        public DateTime? cobertura_vence { get; set; }
        public string pago_frecuencia { get; set; } = string.Empty;
        public int num_seq_inicio { get; set; } = 0;
        public int num_ctas_deduce { get; set; } = 0;
        public DateTime? recaudado_corte { get; set; }
        public decimal recaudado_saldo { get; set; } = 0;

        public string codigo { get; set; } = string.Empty;
        public int cod_destino { get; set; } = 0;
        public string destino { get; set; } = string.Empty;
        public string garantia_codigo { get; set; } = string.Empty;
        public string garantia { get; set; } = string.Empty;
        public string documento { get; set; } = string.Empty;
        public int plazo { get; set; } = 0;
        public decimal monto_base { get; set; } = 0;
        public string observacion { get; set; } = string.Empty;
        public DateTime? fechaforp { get; set; }
        public decimal pagado { get; set; } = 0;
        public int plazo_transcurrido { get; set; } = 0;
        public long prideduc { get; set; } = 0;
    }
}