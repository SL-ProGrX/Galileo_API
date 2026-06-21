namespace Galileo_API.Models.ProGrX.Creditos
{
    public class CrConsultaCrdExcInicialDto
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string linea { get; set; } = string.Empty;
        public decimal mora { get; set; }
        public short condicion { get; set; }
        public string mensaje_condicion { get; set; } = string.Empty;
        public CrConsultaCrdExcResumenDto resumen { get; set; } = new();
        public List<CrConsultaCrdExcBancoDto> bancos { get; set; } = new();
        public List<CrConsultaCrdExcRecursoDto> recursos { get; set; } = new();
        public List<CrConsultaCrdExcTipoDocumentoDto> tipos_documento { get; set; } = new();
    }

    public class CrConsultaCrdExcResumenDto
    {
        public DateTime? periodo_de { get; set; }
        public DateTime? periodo_hasta { get; set; }
        public short mes_aplicado { get; set; }
        public decimal bruto { get; set; }
        public decimal por_cap_gen { get; set; }
        public decimal capitalizacion { get; set; }
        public decimal por_renta { get; set; }
        public decimal renta { get; set; }
        public decimal por_acumulado { get; set; }
        public decimal base_credito { get; set; }
        public decimal saldos { get; set; }
        public decimal por_cap_ind { get; set; }
        public decimal cap_individual { get; set; }
        public decimal neto { get; set; }
        public short dias { get; set; }
        public decimal tasa { get; set; }
        public decimal intereses { get; set; }
        public decimal giro_maximo { get; set; }
        public string nombre { get; set; } = string.Empty;
        public decimal poliza_factor { get; set; }
    }

    public class CrConsultaCrdExcBancoDto
    {
        public int id_banco { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public string desc_corta { get; set; } = string.Empty;
        public string cta { get; set; } = string.Empty;
        public string cod_divisa { get; set; } = string.Empty;
        public int IdX { get; set; }
        public string ItmX { get; set; } = string.Empty;
    }

    public class CrConsultaCrdExcCuentaBancoDto
    {
        public string cuenta_interna { get; set; } = string.Empty;
        public string cuenta_desc { get; set; } = string.Empty;
        public string IdX { get; set; } = string.Empty;
        public string ItmX { get; set; } = string.Empty;
        public int prioridad { get; set; }
    }

    public class CrConsultaCrdExcRecursoDto
    {
        public string IdX { get; set; } = string.Empty;
        public string ItmX { get; set; } = string.Empty;
    }

    public class CrConsultaCrdExcTipoDocumentoDto
    {
        public string item { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class CrConsultaCrdExcDisponibleRecursoDto
    {
        public decimal disponible { get; set; }
    }

    public class CrConsultaCrdExcFormalizarRequest
    {
        public string cedula { get; set; } = string.Empty;
        public string linea { get; set; } = string.Empty;
        public decimal? monto { get; set; }
        public int? banco { get; set; }
        public string tipo_documento { get; set; } = string.Empty;
        public string cuenta_banco { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public string oficina { get; set; } = string.Empty;
        public string app_cod { get; set; } = string.Empty;
    }

    public class CrConsultaCrdExcFormalizarDto
    {
        public int operacion { get; set; }
    }
    public class CrConsultaCrdExcOficinaUsuarioDto
    {
        public string titular { get; set; } = string.Empty;
        public string apoyo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string cod_unidad { get; set; } = string.Empty;
        public string cod_centro_costo { get; set; } = string.Empty;
    }
}