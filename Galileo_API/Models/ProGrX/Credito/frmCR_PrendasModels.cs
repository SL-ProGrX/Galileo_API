namespace Galileo_API.Models.ProGrX.Credito
{
    public class CrPrendaListaData
    {
        public long prenda_id { get; set; } = 0;
        public string tipo_prenda { get; set; } = string.Empty;
        public string tipo_prenda_desc { get; set; } = string.Empty;
        public decimal avaluo { get; set; } = 0;
        public decimal porc_cobertura { get; set; } = 0;
        public decimal cobertura { get; set; } = 0;
        public string descripcion { get; set; } = string.Empty;
        public string id_principal { get; set; } = string.Empty;
        public string id_provisional { get; set; } = string.Empty;
        public string modelo { get; set; } = string.Empty;
        public string serie { get; set; } = string.Empty;
        public string marca { get; set; } = string.Empty;
        public string anio { get; set; } = string.Empty;
        public string registro_fecha { get; set; } = string.Empty;
        public string registro_usuario { get; set; } = string.Empty;
        public string actualiza_fecha { get; set; } = string.Empty;
        public string actualiza_usuario { get; set; } = string.Empty;
        public string tomo { get; set; } = string.Empty;
        public string folio { get; set; } = string.Empty;
        public string notario { get; set; } = string.Empty;
        public string notario_registro_fecha { get; set; } = string.Empty;
    }

    public class CrPrendaDetalleData : CrPrendaListaData
    {
        public string color { get; set; } = string.Empty;
        public string observaciones { get; set; } = string.Empty;
        public string chasis_numero { get; set; } = string.Empty;
        public string vin_motor { get; set; } = string.Empty;
        public decimal puertas_numero { get; set; } = 0;
        public decimal peso { get; set; } = 0;
        public decimal capacidad { get; set; } = 0;
        public decimal cilindraje { get; set; } = 0;
        public decimal valor_fiscal { get; set; } = 0;
        public decimal monto_extras { get; set; } = 0;
        public string avaluo_observacion { get; set; } = string.Empty;
        public string avaluo_inspeccion { get; set; } = string.Empty;
        public string avaluo_inspector { get; set; } = string.Empty;
        public decimal poliza_mnt_formalizacion { get; set; } = 0;
        public decimal poliza_rst_plan { get; set; } = 0;
        public string notario_registro_usuario { get; set; } = string.Empty;
        public string notario_actualiza_usuario { get; set; } = string.Empty;
        public string notario_actualiza_fecha { get; set; } = string.Empty;
        public string combustible_desc { get; set; } = string.Empty;
        public string comercializa_desc { get; set; } = string.Empty;
        public string marca_desc { get; set; } = string.Empty;
        public string modelo_desc { get; set; } = string.Empty;
        public string presentacion_desc { get; set; } = string.Empty;
        public int id_combustible { get; set; } = 0;
        public int id_comercio { get; set; } = 0;
        public int id_marca { get; set; } = 0;
        public int id_modelo { get; set; } = 0;
        public int id_presentacion { get; set; } = 0;
        public string peso_ud { get; set; } = string.Empty;
        public string capacidad_ud { get; set; } = string.Empty;
        public string cilindraje_ud { get; set; } = string.Empty;
        public string peso_ud_desc { get; set; } = string.Empty;
        public string capacidad_ud_desc { get; set; } = string.Empty;
        public string cilindraje_ud_desc { get; set; } = string.Empty;
        public string titular_nombre { get; set; } = string.Empty;
        public int titular_tercero { get; set; } = 0;
        public int pe_indica { get; set; } = 0;
        public int pe_id { get; set; } = 0;
        public string pe_numero { get; set; } = string.Empty;
        public decimal pe_prima { get; set; } = 0;
        public string pe_frecuencia { get; set; } = string.Empty;
        public string pe_inicio { get; set; } = string.Empty;
        public string pe_vence { get; set; } = string.Empty;
        public int pe_activa { get; set; } = 0;
        public string pe_cobertura { get; set; } = string.Empty;
        public string pe_notas { get; set; } = string.Empty;
        public string aseguradora_desc { get; set; } = string.Empty;
        public int id_aseguradora { get; set; } = 0;
        public string a_cedula { get; set; } = string.Empty;
        public string a_tipo_id { get; set; } = string.Empty;
        public string a_tipo_id_desc { get; set; } = string.Empty;
        public string a_apellido_1 { get; set; } = string.Empty;
        public string a_apellido_2 { get; set; } = string.Empty;
        public string a_nombre { get; set; } = string.Empty;
        public string a_email { get; set; } = string.Empty;
        public string a_tel_movil { get; set; } = string.Empty;
        public string a_nacimiento { get; set; } = string.Empty;
        public string a_sexo { get; set; } = string.Empty;
        public string a_parentesco_desc { get; set; } = string.Empty;
        public string a_cod_parentesco { get; set; } = string.Empty;
        public int pe_vencida { get; set; } = 0;
        public string pe_status { get; set; } = string.Empty;
    }

    public class CrPrendaTipoListaData
    {
        public string idx { get; set; } = string.Empty;
        public string itmx { get; set; } = string.Empty;
        public int largo_minimo { get; set; } = 0;
    }

    public class CrPrendaAnotacionData
    {
        public long id_nota { get; set; } = 0;
        public string notas { get; set; } = string.Empty;
        public string registro_fecha { get; set; } = string.Empty;
        public string registro_usuario { get; set; } = string.Empty;
    }

    public class CrPrendaPolizaCoberturaData
    {
        public long id_prenda_cobertura { get; set; } = 0;
        public string cobertura { get; set; } = string.Empty;
        public int asignado { get; set; } = 0;
    }

    public class CrPrendaHistoricoAvaluoData
    {
        public long id_avaluo_h { get; set; } = 0;
        public long prenda_id { get; set; } = 0;
        public string inspector { get; set; } = string.Empty;
        public string fecha_inspeccion { get; set; } = string.Empty;
        public decimal valor_mercado { get; set; } = 0;
        public decimal valor_fiscal { get; set; } = 0;
        public string observaciones { get; set; } = string.Empty;
        public string registro_fecha { get; set; } = string.Empty;
        public string registro_usuario { get; set; } = string.Empty;
        public string actualiza_fecha { get; set; } = string.Empty;
        public string actualiza_usuario { get; set; } = string.Empty;
    }

    public class CrPrendaHistoricoPolizaData
    {
        public int pe_id { get; set; } = 0;
        public long prenda_id { get; set; } = 0;
        public string pe_numero { get; set; } = string.Empty;
        public int pe_activa { get; set; } = 0;
        public string pe_frecuencia { get; set; } = string.Empty;
        public decimal pe_prima { get; set; } = 0;
        public string pe_inicio { get; set; } = string.Empty;
        public string pe_vence { get; set; } = string.Empty;
        public int pe_vencida { get; set; } = 0;
        public int id_aseguradora { get; set; } = 0;
        public string aseguradora_desc { get; set; } = string.Empty;
        public string a_cedula { get; set; } = string.Empty;
        public string asegurado { get; set; } = string.Empty;
        public string a_parentesco_desc { get; set; } = string.Empty;
        public string a_tel_movil { get; set; } = string.Empty;
        public string a_email { get; set; } = string.Empty;
        public string pe_cobertura { get; set; } = string.Empty;
        public string pe_notas { get; set; } = string.Empty;
        public string registro_fecha { get; set; } = string.Empty;
        public string registro_usuario { get; set; } = string.Empty;
        public string actualiza_fecha { get; set; } = string.Empty;
        public string actualiza_usuario { get; set; } = string.Empty;
    }

    public class CrPrendasEliminarRequest
    {
        public long prenda_id { get; set; } = 0;
        public string usuario { get; set; } = string.Empty;
    }

    public class CrPrendasGuardarRequest : CrPrendasEliminarRequest
    {
    }

    public class CrPrendaPolizaExternaGuardarRequest
    {
        public long prenda_id { get; set; } = 0;
        public int id_aseguradora { get; set; } = 0;
        public string pe_numero { get; set; } = string.Empty;
        public decimal pe_prima { get; set; } = 0;
        public string pe_frecuencia { get; set; } = string.Empty;
        public string pe_inicio { get; set; } = string.Empty;
        public string pe_vence { get; set; } = string.Empty;
        public int pe_activa { get; set; } = 0;
        public int pe_indica { get; set; } = 0;
        public string pe_cobertura { get; set; } = string.Empty;
        public string pe_notas { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public string a_tipo_id { get; set; } = string.Empty;
        public string a_cedula { get; set; } = string.Empty;
        public string a_apellido_1 { get; set; } = string.Empty;
        public string a_apellido_2 { get; set; } = string.Empty;
        public string a_nombre { get; set; } = string.Empty;
        public string a_nacimiento { get; set; } = string.Empty;
        public string a_sexo { get; set; } = string.Empty;
        public string a_email { get; set; } = string.Empty;
        public string a_tel_movil { get; set; } = string.Empty;
        public string a_cod_parentesco { get; set; } = string.Empty;
        public int pe_id { get; set; } = 0;
    }

    public class CrPrendaGuardarCompletaRequest
    {
        public long prenda_id { get; set; } = 0;
        public long operacion { get; set; } = 0;
        public string expediente { get; set; } = string.Empty;
        public string identificacion { get; set; } = string.Empty;
        public string tipo_prenda { get; set; } = string.Empty;
        public string id_principal { get; set; } = string.Empty;
        public string id_provisional { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string observaciones { get; set; } = string.Empty;
        public string marca { get; set; } = string.Empty;
        public string modelo { get; set; } = string.Empty;
        public string serie { get; set; } = string.Empty;
        public string color { get; set; } = string.Empty;
        public string anio { get; set; } = string.Empty;
        public decimal peso { get; set; } = 0;
        public decimal capacidad { get; set; } = 0;
        public decimal cilindraje { get; set; } = 0;
        public decimal puertas_numero { get; set; } = 0;
        public string chasis_numero { get; set; } = string.Empty;
        public string vin_motor { get; set; } = string.Empty;
        public int id_marca { get; set; } = 0;
        public int id_modelo { get; set; } = 0;
        public int id_presentacion { get; set; } = 0;
        public int id_combustible { get; set; } = 0;
        public int id_comercio { get; set; } = 0;
        public string peso_ud { get; set; } = string.Empty;
        public string capacidad_ud { get; set; } = string.Empty;
        public string cilindraje_ud { get; set; } = string.Empty;
        public decimal avaluo { get; set; } = 0;
        public decimal porc_cobertura { get; set; } = 0;
        public decimal cobertura { get; set; } = 0;
        public string avaluo_observacion { get; set; } = string.Empty;
        public string avaluo_inspeccion { get; set; } = string.Empty;
        public decimal valor_fiscal { get; set; } = 0;
        public decimal monto_extras { get; set; } = 0;
        public int poliza_factor { get; set; } = 0;
        public decimal poliza_mnt_formalizacion { get; set; } = 0;
        public decimal poliza_rst_plan { get; set; } = 0;
        public string usuario { get; set; } = string.Empty;
        public int titular_tercero { get; set; } = 0;
        public string titular_nombre { get; set; } = string.Empty;
        public string avaluo_inspector { get; set; } = string.Empty;
        public bool es_vehicular { get; set; } = false;
    }

    public class CrPrendaAvaluoGuardarRequest
    {
        public long prenda_id { get; set; } = 0;
        public string inspector { get; set; } = string.Empty;
        public decimal valor_total { get; set; } = 0;
        public decimal cobertura { get; set; } = 0;
        public decimal porc_cobertura { get; set; } = 0;
        public string notas { get; set; } = string.Empty;
        public string fecha_inspeccion { get; set; } = string.Empty;
        public decimal valor_fiscal { get; set; } = 0;
        public decimal monto_extras { get; set; } = 0;
        public int poliza_factor { get; set; } = 0;
        public decimal poliza_formaliza { get; set; } = 0;
        public decimal poliza_rst_plan { get; set; } = 0;
        public string usuario { get; set; } = string.Empty;
    }

    public class CrPrendaNotariadoGuardarRequest
    {
        public long prenda_id { get; set; } = 0;
        public string notario { get; set; } = string.Empty;
        public string tomo { get; set; } = string.Empty;
        public string folio { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
    }

    public class CrPrendaNotaGuardarRequest
    {
        public long prenda_id { get; set; } = 0;
        public string nota { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
    }

    public class CrPrendaPolizaCoberturaGuardarRequest
    {
        public long prenda_id { get; set; } = 0;
        public long id_prenda_cobertura { get; set; } = 0;
        public string usuario { get; set; } = string.Empty;
        public int asignado { get; set; } = 0;
    }
}
