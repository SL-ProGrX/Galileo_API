using Galileo.Models;

namespace Galileo_API.Models.ProGrX.Creditos
{
    public class CrRetencionCargadoPantallaData
    {
        public List<DropDownListaGenericaModel> clientes { get; set; } = new();
        public List<DropDownListaGenericaModel> instituciones { get; set; } = new();
        public List<DropDownListaGenericaModel> tipos_deduccion { get; set; } = new();
        public List<DropDownListaGenericaModel> procesos { get; set; } = new();
        public string proceso_default { get; set; } = string.Empty;
        public string tipo_deduccion_default { get; set; } = "I";
        public bool archivo_excel_default { get; set; } = true;
        public bool revisar_institucion_default { get; set; } = true;
    }

    public class CrRetencionCargadoDeductoraDetalleData
    {
        public List<DropDownListaGenericaModel> frecuencias { get; set; } = new();
        public string frecuencia_id { get; set; } = "0";
        public string frecuencia_descripcion { get; set; } = "Mensual";
        public string primer_deduccion { get; set; } = string.Empty;
    }

    public class CrRetencionCargadoCargaRequest
    {
        public string codigo { get; set; } = string.Empty;
        public int cod_institucion { get; set; }
        public int cod_deductora { get; set; }
        public string proceso { get; set; } = string.Empty;
        public string tipo_deduccion { get; set; } = "I";
        public bool archivo_excel { get; set; } = true;
        public bool revisar_institucion { get; set; } = true;
        public List<CrRetencionCargadoCargaItemRequest> items { get; set; } = new();
    }

    public class CrRetencionCargadoCargaItemRequest
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public decimal? cuota { get; set; }
        public string operacion { get; set; } = string.Empty;
        public DateTime? formalizacion { get; set; }
        public string movimiento { get; set; } = string.Empty;
        public int? plazo { get; set; }
    }

    public class CrRetencionCargadoCargaData
    {
        public List<CrRetencionCargadoDetalleData> detalle { get; set; } = new();
        public CrRetencionCargadoTotalesData totales { get; set; } = new();
    }

    public class CrRetencionCargadoDetalleData
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public string movimiento_name { get; set; } = string.Empty;
        public string existe_inst { get; set; } = string.Empty;
        public int plazo { get; set; }
        public decimal cuota { get; set; }
        public string operacion { get; set; } = string.Empty;
        public DateTime? formaliza { get; set; }
    }

    public class CrRetencionCargadoTotalesData
    {
        public decimal monto { get; set; }
        public int casos { get; set; }
        public int inclusion { get; set; }
        public int exclusion { get; set; }
        public int cambio { get; set; }
        public int errores { get; set; }
    }

    public class CrRetencionCargadoAplicarRequest
    {
        public string codigo { get; set; } = string.Empty;
        public int cod_institucion { get; set; }
        public string proceso { get; set; } = string.Empty;
        public string frecuencia_id { get; set; } = "0";
        public List<CrRetencionCargadoAplicarDetalleRequest> detalle { get; set; } = new();
    }

    public class CrRetencionCargadoAplicarDetalleRequest
    {
        public string cedula { get; set; } = string.Empty;
        public string existe_inst { get; set; } = string.Empty;
    }
}