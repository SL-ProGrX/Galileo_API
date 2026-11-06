namespace Galileo_API.Models.ProGrX_Polizas
{
  
    public class CrPolizaMacHogarEnvioConsultaRequest: PolizaEnvioConsultaRequestBase
    {
        public string pMacH1 { get; set; } = string.Empty;
    }

    public class CrPolizaMacHogarEnvioRow
    {
        public object? corte { get; set; }
        public object? direccion_riesgo { get; set; }
        public object? provincia_riesgo { get; set; }
        public object? canton_riesgo { get; set; }
        public object? distrito_riesgo { get; set; }
        public object? folio_real { get; set; }
        public object? monto_residencia { get; set; }
        public object? monto_otras_estructuras { get; set; }
        public object? monto_familiar { get; set; }
        public object? cob_d { get; set; }
        public object? cob_x { get; set; }
        public object? cob_y { get; set; }
        public object? correo_electronico { get; set; }
        public object? incluir_cobertura { get; set; }
        public object? latitud { get; set; }
        public object? longitud { get; set; }
        public object? pisos { get; set; }
        public object? tipo_id { get; set; }
        public object? identificacion { get; set; }
        public object? primer_apellido { get; set; }
        public object? segundo_apellido { get; set; }
        public object? primer_nombre { get; set; }
        public object? segundo_nombre { get; set; }
        public object? direccion_completa { get; set; }
        public object? fecha_nacimiento { get; set; }
        public object? genero { get; set; }
        public object? telefono { get; set; }
        public object? correo_persona { get; set; }
        public object? pais_buc_num { get; set; }
        public object? anos_construccion { get; set; }
        public object? bien_asegurado { get; set; }
        public object? es_esquina { get; set; }
        public object? tipo_ocupacion { get; set; }
        public object? tipo_construccion { get; set; }
        public object? cob_v { get; set; }
        public object? no_aplica { get; set; }
        public object? aseguramiento_m { get; set; }
        public object? aseguramiento_p1 { get; set; }
        public object? aseguramiento_p2 { get; set; }
        public object? aseguramiento_p3 { get; set; }
        public object? aseguramiento_x { get; set; }
        public object? aseguramiento_y { get; set; }
        public object? tarifa_cob_d { get; set; }
        public object? tarifa_cob_v { get; set; }
        public object? tarifa_cob_y { get; set; }
        public object? monto_propiedad_personal { get; set; }
        public object? solvencia { get; set; }
        public object? poliza_migrada { get; set; }
        public object? numero_referencia { get; set; }
        public object? agente { get; set; }
        public object? nombre_completo { get; set; }
        public object? sucursal_emision { get; set; }
        public object? numero_contrato { get; set; }
        public object? forma_pago { get; set; }
        public object? base_aseguramiento { get; set; }
        public object? descripcion { get; set; }
        public object? agrupador_riesgo { get; set; }
        public object? cedula_intermediario { get; set; }
        public object? observaciones_poliza { get; set; }
        public object? numero_poliza { get; set; }
        public object? razon_cancelacion { get; set; }
        public object? migracion_cb { get; set; }
        public object? mco { get; set; }
        public object? sys { get; set; }
        public object? poliza { get; set; }
        public object? mod { get; set; }
        public object? tipo_variacion_incendio { get; set; }
        public object? fecha_formalizacion { get; set; }
        public object? tipo_identificacion { get; set; }
        public object? identificacion_acreedor { get; set; }
        public object? nombre_acreedor { get; set; }
        public object? monto { get; set; }
        public object? rubro_acreencia { get; set; }
        public object? observaciones { get; set; }
        public object? movimiento { get; set; }
        public object? nacionalidad_descripcion { get; set; }
        public object? nacionalidad_codigo_alterno { get; set; }
    }

    public class CrPolizaMacHogarBeneficiariosRowDto : PolizaBeneficiariosRowBase
    {
        public string pMacH3 { get; set; } = string.Empty;
    }

    public abstract class BeneficiariosB1B6MacHogar : PolizaBeneficiariosB1B6Base
    {
        public string pMacH6 { get; set; } = string.Empty;
    }

}
