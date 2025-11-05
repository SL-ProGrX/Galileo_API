namespace PgxAPI.Models.ProGrX.Bancos
{
    public class TesAutoRegistroLista
    {
        public int total { get; set; } = 0;
        public List<TesAutoRegistroDto> lista { get; set; } = new List<TesAutoRegistroDto>();
    }

    public class TesAutoRegistroDto
    {
        public int? id_auto { get; set; }
        public string? descripcion { get; set; }
        public string? palabras_clave { get; set; }
        public string? detalle { get; set; }
        public string? cod_concepto { get; set; }
        public string? cod_cuenta { get; set; }
        public string? cod_unidad { get; set; }
        public string? cod_centro_costo { get; set; }
        public float? mnt_inicio { get; set; }
        public float? mnt_corte { get; set; }
        public bool? apl_carga_diaria { get; set; }
        public bool? apl_conciliacion { get; set; }
        public bool? ind_info_persona { get; set; }
        public string? tipo_beneficiario { get; set; }
        public string? beneficiario_id { get; set; }
        public string? beneficiario_nombre { get; set; }
        public bool? activo { get; set; }
        public Nullable<DateTime> registro_fecha { get; set; }
        public string? registro_usuario { get; set; }
        public string? modifica_usuario { get; set; }
        public Nullable<DateTime> modifica_fecha { get; set; }
        public string? apl_tipo_mov { get; set; }
        public bool? filtra_cta_bancos { get; set; }
        public string? tipo_doc { get; set; }
        public bool? ignora_registro { get; set; }
        public string? concepto_desc { get; set; }
        public string? cod_cuenta_mask { get; set; }
        public string? cuenta_desc { get; set; }
        public string? unidad_desc { get; set; }
        public string? centro_desc { get; set; }
        public string? apl_tipo_mov_desc { get; set; }
        public string? tipo_doc_id { get; set; }
        public string? tipo_doc_desc { get; set; }
        public bool? ignora_registro_id { get; set; }
        public bool? dp_tramite { get; set; }
    }

    public class TesAutoRegCtaBancariasData
    {
        public int id_banco { get; set; }
        public string? cta { get; set; }
        public string? descripcion { get; set; }
        public string? desc_corta { get; set; }
        public string? cod_divisa { get; set; }
        public bool asignado { get; set; }
        public string? cod_cuenta_mask { get; set; }
    }

    public class TesAutoregistroConceptos 
    {
        public string? cod_concepto { get; set; }
        public string? descripcion { get; set; }
        public string? cod_cuenta_mask { get; set; }
        public int dp_tramite_apl { get; set; }
        public string? cuenta_desc { get; set; }
    }

    public class TipoMovData
    {
        public string? tipo { get; set; }
        public string? descripcion { get; set; }
    }

    public class AutoRegGuardar
    {
        public int auto_id { get; set; }
        public string? usuario { get; set; }
        public DateTime fecha { get; set; }
        public string? result { get; set; }
    }
}