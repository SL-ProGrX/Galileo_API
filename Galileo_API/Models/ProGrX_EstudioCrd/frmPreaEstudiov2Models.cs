namespace Galileo_API.Models.ProGrX_EstudioCrd
{
    namespace Galileo_API.Models.ProGrX_EstudioCrd
    {
        public class FrmPreaEstudiov2CargaRequest
        {
            public string usuario { get; set; } = string.Empty;
            public string cod_preanalisis { get; set; } = string.Empty;
        }

        public class FrmPreaEstudiov2ScrollRequest
        {
            public string usuario { get; set; } = string.Empty;
            public string cod_preanalisis { get; set; } = string.Empty;
            public int scroll_code { get; set; }
        }

        public class FrmPreaEstudiov2CatalogosResponse
        {
            public List<FrmPreaEstudiov2DropdownDto> expedientes { get; set; } = [];
            public List<FrmPreaEstudiov2DropdownDto> lineas { get; set; } = [];
            public List<FrmPreaEstudiov2DropdownDto> destinos { get; set; } = [];
            public List<FrmPreaEstudiov2DropdownDto> garantias { get; set; } = [];
            public List<FrmPreaEstudiov2DropdownDto> tipos_salario { get; set; } = [];
            public List<FrmPreaEstudiov2DropdownDto> componentes_adicionales { get; set; } = [];
        }

        public class FrmPreaEstudiov2DropdownDto
        {
            public string item { get; set; } = string.Empty;
            public string descripcion { get; set; } = string.Empty;
        }

        public class FrmPreaEstudiov2CargaResponse
        {
            public FrmPreaEstudiov2EstadoDto estado { get; set; } = new();
            public FrmPreaEstudiov2EncabezadoDto encabezado { get; set; } = new();
            public FrmPreaEstudiov2CreditoDto credito { get; set; } = new();
            public FrmPreaEstudiov2SalariosDto salarios { get; set; } = new();
            public FrmPreaEstudiov2CatalogosResponse catalogos { get; set; } = new();
        }

        public class FrmPreaEstudiov2EstadoDto
        {
            public string cod_preanalisis { get; set; } = string.Empty;
            public string estado { get; set; } = string.Empty;
            public string estado_desc { get; set; } = string.Empty;
            public string estado_v2 { get; set; } = string.Empty;
            public string estado_v2_desc { get; set; } = string.Empty;
            public bool editable { get; set; }
            public bool tiene_alerta { get; set; }
            public string mensaje_alerta { get; set; } = string.Empty;
        }

        public class FrmPreaEstudiov2EncabezadoDto
        {
            public string cedula { get; set; } = string.Empty;
            public string nombre { get; set; } = string.Empty;
            public string sexo { get; set; } = string.Empty;
            public DateTime? fecha_nacimiento { get; set; }
            public int edad { get; set; }
            public string estado_persona { get; set; } = string.Empty;
            public string clasificacion_crediticia { get; set; } = string.Empty;
            public int edad_aplica { get; set; }
            public string edad_justificacion { get; set; } = string.Empty;
        }

        public class FrmPreaEstudiov2CreditoDto
        {
            public string linea { get; set; } = string.Empty;
            public string destino { get; set; } = string.Empty;
            public string garantia { get; set; } = string.Empty;
            public int fiadores { get; set; }
            public string contrato { get; set; } = string.Empty;
            public string no_op_crm { get; set; } = string.Empty;
            public decimal monto { get; set; }
            public decimal tasa { get; set; }
            public int plazo { get; set; }
            public decimal cuota { get; set; }
            public decimal monto_construccion { get; set; }
            public bool poliza_vida { get; set; }
            public bool poliza_incendio { get; set; }
            public bool poliza_prenda { get; set; }
            public bool poliza_desempleo { get; set; }
            public decimal monto_poliza_vida { get; set; }
            public decimal monto_poliza_incendio { get; set; }
            public decimal monto_poliza_prenda { get; set; }
            public decimal monto_poliza_desempleo { get; set; }
            public decimal compromiso { get; set; }
        }

        public class FrmPreaEstudiov2SalariosDto
        {
            public string tipo_salario { get; set; } = string.Empty;
            public DateTime? corte_colilla { get; set; }
            public decimal salario_devengado { get; set; }
            public decimal salario_mensual { get; set; }
            public decimal salario_constancia { get; set; }
            public decimal salario_orden_patronal { get; set; }
            public decimal ingreso_privado { get; set; }
            public decimal ingreso_privado_porc { get; set; }
            public int componente_adicional_id { get; set; }
            public decimal componente_adicional_porc { get; set; }
            public decimal componentes_adicionales { get; set; }
            public decimal total_extras { get; set; }
            public List<FrmPreaEstudiov2SalarioDetalleDto> tabla_salarios { get; set; } = [];
            public List<FrmPreaEstudiov2ExtraDto> extras { get; set; } = [];
            public List<FrmPreaEstudiov2IncapacidadDto> incapacidades { get; set; } = [];
        }

        public class FrmPreaEstudiov2SalarioDetalleDto
        {
            public int orden { get; set; }
            public DateTime? fecha { get; set; }
            public decimal salario_s { get; set; }
            public int mes { get; set; }
            public decimal salario_rh { get; set; }
            public decimal ca { get; set; }
        }

        public class FrmPreaEstudiov2ExtraDto
        {
            public int idx { get; set; }
            public string cod_extras { get; set; } = string.Empty;
            public string tipo_extra { get; set; } = string.Empty;
            public decimal monto { get; set; }
        }

        public class FrmPreaEstudiov2IncapacidadDto
        {
            public int orden { get; set; }
            public DateTime? desde { get; set; }
            public DateTime? hasta { get; set; }
            public int dias { get; set; }
        }
    }

    public class FrmPreaEstudiov2HipotecarioRequest
    {
        public string cod_preanalisis { get; set; } = string.Empty;
        public long id_solicitud { get; set; }
        public string usuario { get; set; } = string.Empty;
    }

    public class FrmPreaEstudiov2HipotecarioResponse
    {
        public decimal monto_avaluo_factor_cfia { get; set; }
        public bool habilita_montos_hipoteca { get; set; }
        public bool habilita_sumar_avaluo_cfia { get; set; }
        public bool habilita_garantia_hipoteca { get; set; }
        public bool habilita_asignar_ingenieros { get; set; }
        public bool habilita_cambio_estado { get; set; }
        public string mensaje_bloqueo { get; set; } = string.Empty;
    }

}
