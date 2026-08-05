using Galileo.Models;

namespace Galileo_API.Models.ProGrX.Creditos
{
    public class CrCorreccionCreditosConsultaResponse
    {
        public CrCorreccionCreditosOperacion operacion { get; set; } = new();
        public List<CrCorreccionCreditosMovimiento> movimientos { get; set; } = [];
    }

    public class CrCorreccionCreditosOperacion
    {
        public int id_solicitud { get; set; }
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string estado_descripcion { get; set; } = string.Empty;
        public string opex_descripcion { get; set; } = string.Empty;
        public decimal montoapr { get; set; }
        public decimal saldo { get; set; }
        public int plazo { get; set; }
        public int plazo_restante { get; set; }
        public decimal interes { get; set; }
        public decimal tasa_original { get; set; }
        public decimal cuota { get; set; }
        public int fecult { get; set; }
        public int prideduc { get; set; }
        public string garantia { get; set; } = string.Empty;
        public string garantia_descripcion { get; set; } = string.Empty;
        public string cod_destino { get; set; } = string.Empty;
        public string destino_descripcion { get; set; } = string.Empty;
        public string cod_grupo { get; set; } = string.Empty;
        public string recurso_descripcion { get; set; } = string.Empty;
        public string cod_oficina_r { get; set; } = string.Empty;
        public string oficina_descripcion { get; set; } = string.Empty;
        public int? id_promotor { get; set; }
        public string ejecutivo_descripcion { get; set; } = string.Empty;
        public string cod_actividad { get; set; } = string.Empty;
        public int dia_pago { get; set; }
        public decimal? tbp_puntos_add { get; set; }
        public bool liq_tasa { get; set; }
        public bool retencion { get; set; }
        public bool sys_plan_pagos { get; set; }
        public string base_calculo { get; set; } = string.Empty;
    }

    public class CrCorreccionCreditosMovimiento
    {
        public int id { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public string tipo_editor { get; set; } = string.Empty;
    }

    public class CrCorreccionCreditosDetalleSeleccion
    {
        public int id { get; set; }
        public string proceso { get; set; } = string.Empty;
        public DateTime? fecha { get; set; }
        public string usuario { get; set; } = string.Empty;
        public decimal int_cor { get; set; }
        public decimal int_mor { get; set; }
        public decimal principal { get; set; }
        public decimal cargos { get; set; }
        public decimal monto { get; set; }
        public string dias { get; set; } = string.Empty;
        public string detalle { get; set; } = string.Empty;
        public int id_mora { get; set; }
        public bool seleccionado { get; set; }
    }

    public class CrCorreccionCreditosAplicarRequest
    {
        public required int operacion { get; set; }
        public required int movimiento { get; set; }
        public string valor { get; set; } = string.Empty;
        public decimal? valor_numerico { get; set; }
        public decimal? tasa { get; set; }
        public decimal? tbp_puntos_add { get; set; }
        public required bool tasa_indizada_tbp { get; set; }
        public required bool aplica_puntos_renuncia { get; set; }
        public required bool ajustar_primer_deduccion { get; set; }
        public string notas { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public List<int> seleccionados { get; set; } = [];
    }

    public class CrCorreccionCreditosAnularRequest
    {
        public required int operacion { get; set; }
        public string usuario { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
    }

    public class CrCorreccionCreditosExcluirRequest
    {
        public required int operacion { get; set; }
        public string usuario { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
    }

    public class CrCorreccionCreditosResultado
    {
        public string mensaje { get; set; } = string.Empty;
        public string tipo_documento { get; set; } = string.Empty;
        public int numero_documento { get; set; }
        public string? reporte_resultado { get; set; }
    }
}
