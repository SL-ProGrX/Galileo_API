namespace Galileo.Models
{
    public sealed class MAfilicacionDireccionValidarRequest
    {
        public string direccion { get; set; } = string.Empty;

        public string caracteres_relleno { get; set; } = string.Empty;

        public int cantidad_palabras { get; set; } = 3;

        public int largo_direccion { get; set; } = 20;
    }

    public sealed class MAfilicacionParametrosData
    {
        public int largo_cedula { get; set; } = 0;

        public bool solicitar_telefonos { get; set; } = false;

        public bool solicitar_cuentas { get; set; } = false;

        public bool solicitar_beneficiario { get; set; } = false;

        public bool verifica_nombre { get; set; } = false;

        public bool verifica_padron { get; set; } = false;

        public bool bitacora_especial { get; set; } = false;
    }

    public sealed class MAfilicacionSeccionRequest
    {
        public int cod_institucion { get; set; } = 0;

        public string cod_departamento { get; set; } = string.Empty;

        public string cod_seccion { get; set; } = string.Empty;

        public bool sys_ase_version { get; set; } = false;
    }

    public sealed class MAfilicacionReIngresoRequest
    {
        public string cedula { get; set; } = string.Empty;

        public int cod_institucion { get; set; } = 0;

        public int cod_promotor { get; set; } = 0;

        public string boleta { get; set; } = string.Empty;

        public DateTime fecha_ingreso { get; set; } = default;

        public string usuario { get; set; } = string.Empty;

        public string oficina { get; set; } = string.Empty;
    }

    public class MAfilicacionCreditoTasaData
    {
        public long id_solicitud { get; set; } = 0;

        public decimal saldo { get; set; } = 0;

        public int? prideduc { get; set; } = null;

        public int? fecult { get; set; } = null;

        public decimal interes { get; set; } = 0;

        public decimal interesv { get; set; } = 0;

        public int plazo { get; set; } = 0;
    }

    public sealed class MAfilicacionLiquidacionTasaData
    {
        public int liq_alterna { get; set; } = 0;

        public decimal tasa_planilla { get; set; } = 0;

        public decimal tasa_ventanilla { get; set; } = 0;
    }

    public sealed class MAfilicacionCreditoLiquidacionTasaData
        : MAfilicacionCreditoTasaData
    {
        public int opex { get; set; } = 0;

        public int liq_tasa { get; set; } = 0;

        public decimal liq_valor { get; set; } = 0;

        public string liq_tipo_aumento { get; set; } =  string.Empty;

        public string ind_deduce_planilla { get; set; } = string.Empty;
    }
}