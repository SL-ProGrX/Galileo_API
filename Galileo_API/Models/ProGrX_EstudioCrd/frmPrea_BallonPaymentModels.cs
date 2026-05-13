namespace Galileo_API.Models.ProGrX_EstudioCrd
{
    public class FrmPreaBallonPaymentCargarResponse
    {
        public string cod_preanalisis { get; set; } = string.Empty;
        public bool traslada_salario { get; set; }
        public bool deduce_planilla { get; set; }
        public decimal monto { get; set; }
        public decimal cuota { get; set; }
        public decimal cuota_balloon { get; set; }
        public decimal tasa { get; set; }
        public int plazo { get; set; }
        public int periodicidad { get; set; }
        public string periodicidad_desc { get; set; } = string.Empty;
        public string codigo_plan { get; set; } = string.Empty;
        public string no_contrato { get; set; } = string.Empty;
        public string plazo_ahorro { get; set; } = string.Empty;
        public decimal monto_ahorro { get; set; }
        public List<FrmPreaBallonPaymentPeriodicidadItem> periodicidades { get; set; } = new();
    }

    public class FrmPreaBallonPaymentCondicionesGuardarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_preanalisis { get; set; } = string.Empty;
        public bool traslada_salario { get; set; }
        public bool deduce_planilla { get; set; }
    }

    public class FrmPreaBallonPaymentCondicionesGuardarResponse
    {
        public string cod_preanalisis { get; set; } = string.Empty;
        public bool traslada_salario { get; set; }
        public bool deduce_planilla { get; set; }
        public string mensaje { get; set; } = string.Empty;
    }

    public class FrmPreaBallonPaymentCalcularRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_preanalisis { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public decimal tasa { get; set; }
        public int plazo { get; set; }
        public int periodicidad { get; set; }
        public decimal cuota_balloon { get; set; }
    }

    public class FrmPreaBallonPaymentCalcularResponse
    {
        public string cod_preanalisis { get; set; } = string.Empty;
        public decimal cuota { get; set; }
        public string mensaje { get; set; } = string.Empty;
    }

    public class FrmPreaBallonPaymentGuardarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_preanalisis { get; set; } = string.Empty;
        public int periodicidad { get; set; } = 0;
        public decimal tasa { get; set; } = 0;
        public int plazo { get; set; } = 0;
        public decimal cuota_balloon { get; set; } = 0;
        public decimal cuota { get; set; } = 0;
        public decimal monto { get; set; } = 0;
    }

    public class FrmPreaBallonPaymentGuardarResponse
    {
        public string cod_preanalisis { get; set; } = string.Empty;
        public string mensaje { get; set; } = string.Empty;
    }

    public class FrmPreaBallonPaymentTablaPagosResponse
    {
        public List<FrmPreaBallonPaymentTablaPagoItem> tabla { get; set; } = new();
    }

    public class FrmPreaBallonPaymentTablaPagoItem
    {
        public int id_cuota { get; set; } = 0;
        public decimal monto_cuota { get; set; } = 0;
        public decimal amortiza { get; set; } = 0;
        public decimal intereses { get; set; } = 0;
        public decimal monto_principal { get; set; } = 0;
    }

    public class FrmPreaBallonPaymentPeriodicidadItem
    {
        public int idx { get; set; } = 0;
        public string itmx { get; set; } = string.Empty;
    }
}
