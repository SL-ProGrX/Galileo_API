using Galileo.Models;

namespace Galileo_API.Models.ProGrX.Creditos
{
    public class CrConsultaCreditosMoraHeaderDto
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public DateTime? fecha { get; set; }
    }
    public class CrConsultaCreditosMoraListaRequest
    {
        public string cedula { get; set; } = string.Empty;
        public short cuota_transito { get; set; }
        public FiltrosLazyLoadData filtros { get; set; } = new();
    }
    public class CrConsultaCreditosMoraListaResult<T>
    {
        public int total { get; set; }
        public List<T> lista { get; set; } = new();
        public CrConsultaCreditosMoraTotalesDto totales { get; set; } = new();
    }
    public class CrConsultaCreditosMoraDetalleDto
    {
        public int id_solicitud { get; set; }
        public string codigo { get; set; } = string.Empty;
        public int fecult { get; set; }
        public decimal montoapr { get; set; }
        public string proceso_cod { get; set; } = string.Empty;
        public decimal saldo { get; set; }
        public decimal cuota { get; set; }
        public string proceso { get; set; } = string.Empty;
        public string linea_x { get; set; } = string.Empty;
        public string documento_referido { get; set; } = string.Empty;
        public string ndocumento { get; set; } = string.Empty;
        public string referencia { get; set; } = string.Empty;
        public decimal interesv { get; set; }
        public int plazo { get; set; }
        public decimal tasa_original { get; set; }
        public string garantia { get; set; } = string.Empty;
        public decimal mora_cuota { get; set; }
        public decimal mora_int { get; set; }
        public decimal mora_principal { get; set; }
        public decimal mora_cargos { get; set; }
        public decimal mora_poliza { get; set; }
        public int mora_antigua { get; set; }
        public int mora_ultima { get; set; }
        public string observacion_proceso { get; set; } = string.Empty;
        public DateTime? fecha_enviaproceso { get; set; }
        public DateTime? fechaforp { get; set; }
        public string userfor { get; set; } = string.Empty;
        public string cod_oficina_r { get; set; } = string.Empty;
        public string oficina_x { get; set; } = string.Empty;
        public decimal cbr_intereses { get; set; }
        public string destino_x { get; set; } = string.Empty;
        public int indicador_cbr { get; set; }
        public decimal mora_financiera { get; set; }
        public decimal mora_legal { get; set; }
        public decimal en_cobro_judicial { get; set; }
        public string estado_icono { get; set; } = string.Empty;
        public string estado_nota { get; set; } = string.Empty;
        public string linea_nota { get; set; } = string.Empty;
    }
    public class CrConsultaCreditosMoraGarantiaDto
    {
        public string garantia { get; set; } = string.Empty;
        public decimal saldo { get; set; }
        public int operaciones { get; set; }
        public decimal mor_int_cor { get; set; }
        public decimal mor_int_mor { get; set; }
        public decimal mor_cargos { get; set; }
        public decimal mor_principal { get; set; }
        public decimal mor_cuotas { get; set; }
        public int mor_cta_antigua { get; set; }
        public int mor_cta_ultima { get; set; }
        public int mora_dias { get; set; }
        public string antiguedad { get; set; } = string.Empty;
        public string cod_antiguedad { get; set; } = string.Empty;
        public decimal mora_financiera { get; set; }
        public decimal mora_legal { get; set; }
    }

    public class CrConsultaCreditosMoraTotalesDto
    {
        public decimal no_cuotas { get; set; }
        public decimal intereses_atrasados { get; set; }
        public decimal cargos_registrados { get; set; }
        public decimal polizas_registradas { get; set; }
        public decimal principal_atrasado { get; set; }
        public decimal mora_financiera { get; set; }
        public decimal mora_legal { get; set; }
        public decimal en_cobro_judicial { get; set; }
    }
}