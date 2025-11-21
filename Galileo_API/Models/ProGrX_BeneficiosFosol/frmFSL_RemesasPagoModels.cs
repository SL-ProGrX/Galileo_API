namespace Galileo.Models.FSL
{
    public class FslRemesasLista
    {
        public int Total { get; set; }
        public List<FslRemesasListaDatos>? Lista { get; set; }
    }

    public class FslRemesasListaDatos
    {
        public long tesoreria_remesa { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime registro_fecha { get; set; }
        public DateTime fecha_inicio { get; set; }
        public DateTime fecha_corte { get; set; }
        public string? notas { get; set; }
        public string? estado { get; set; }
        public string? descripcion { get; set; }
    }

    public class FslRemesaInsertar
    {
        public long cod_remesa { get; set; }
        public string usuario { get; set; } = string.Empty;
        public DateTime fecha_inicio { get; set; }
        public DateTime fecha_corte { get; set; }
        public string notas { get; set; } = string.Empty;
    }

    public class FslCargasLista
    {
        public int Total { get; set; }
        public List<FslCargasListaData>? Lista { get; set; }
    }

    public class FslCargasListaData
    {
        public string cod_expediente { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public float total_sobrante { get; set; }
        public string presenta_cedula { get; set; } = string.Empty;
        public string presenta_nombre { get; set; } = string.Empty;
    }

    public class FslTrasladoListaData
    {
        public string cod_expediente { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public float total_sobrante { get; set; }
        public string presenta_cedula { get; set; } = string.Empty;
        public string presenta_nombre { get; set; } = string.Empty;
    }

    public class FslCargasAplicar
    {
        public long cod_remesa { get; set; }
        public List<FslCargasListaData> casos { get; set; } = new List<FslCargasListaData>();
        public string usuario { get; set; } = string.Empty;
    }

    public class FslTrasladoAplicar
    {
        public string usuario { get; set; } = string.Empty;
        public long codTraslado { get; set; }
        public List<FslTrasladoListaData> casos { get; set; } = new List<FslTrasladoListaData>();
    }

    public class FslCuentaAhorrosDatos
    {
        public string id_cuah { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string id_banco { get; set; } = string.Empty;
        public string cuenta { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string prioridad { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public DateTime fecha { get; set; }
        public string estado { get; set; } = string.Empty;

    }

    public class FslTesTransaccionesData
    {
        public long nsolicitud { get; set; }
        public string id_banco { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string codigo { get; set; } = string.Empty;
        public string beneficiario { get; set; } = string.Empty;
        public float monto { get; set; }
        public DateTime fecha_solicitud { get; set; }
        public string estado { get; set; } = string.Empty;
        public DateTime fecha_emision { get; set; }
        public DateTime fecha_anula { get; set; }
        public string estadoi { get; set; } = string.Empty;
        public string modulo { get; set; } = string.Empty;
        public string cta_ahorros { get; set; } = string.Empty;
        public string ndocumento { get; set; } = string.Empty;
        public string detalle1 { get; set; } = string.Empty;
        public string detalle2 { get; set; } = string.Empty;
        public string detalle3 { get; set; } = string.Empty;
        public string detalle4 { get; set; } = string.Empty;
        public string detalle5 { get; set; } = string.Empty;
        public string referencia { get; set; } = string.Empty;
        public string submodulo { get; set; } = string.Empty;
        public string genera { get; set; } = string.Empty;
        public string actualiza { get; set; } = string.Empty;
        public string ubicacion_actual { get; set; } = string.Empty;
        public DateTime fecha_traslado { get; set; }
        public string ubicacion_anterior { get; set; } = string.Empty;
        public string entregado { get; set; } = string.Empty;
        public string autoriza { get; set; } = string.Empty;
        public DateTime fecha_asiento { get; set; }
        public DateTime fecha_asiento2 { get; set; }
        public string estado_asiento { get; set; } = string.Empty;
        public DateTime fecha_autorizacion { get; set; }
        public string user_autoriza { get; set; } = string.Empty;
        public string op { get; set; } = string.Empty;
        public string detalle_anulacion { get; set; } = string.Empty;
        public string user_asiento_emision { get; set; } = string.Empty;
        public string user_asiento_anula { get; set; } = string.Empty;
        public string cod_concepto { get; set; } = string.Empty;
        public string cod_unidad { get; set; } = string.Empty;
        public string user_genera { get; set; } = string.Empty;
        public string user_solicita { get; set; } = string.Empty;
        public string user_anula { get; set; } = string.Empty;
        public string user_entrega { get; set; } = string.Empty;
        public DateTime fecha_entrega { get; set; }
        public string documento_ref { get; set; } = string.Empty;
        public string documento_base { get; set; } = string.Empty;
        public string detalle { get; set; } = string.Empty;
        public string user_hold { get; set; } = string.Empty;
        public DateTime fecha_hold { get; set; }
        public DateTime firmas_autoriza_fecha { get; set; }
        public string firmas_autoriza_usuario { get; set; } = string.Empty;
        public string tipo_cambio { get; set; } = string.Empty;
        public string cod_divisa { get; set; } = string.Empty;
        public string tipo_beneficiario { get; set; } = string.Empty;
        public string cod_app { get; set; } = string.Empty;
        public string ref_01 { get; set; } = string.Empty;
        public string ref_02 { get; set; } = string.Empty;
        public string ref_03 { get; set; } = string.Empty;
        public string id_token { get; set; } = string.Empty;
        public string remesa_tipo { get; set; } = string.Empty;
        public string remesa_id { get; set; } = string.Empty;
        public string asiento_numero { get; set; } = string.Empty;
        public string asiento_numero_anu { get; set; } = string.Empty;
        public string concilia_id { get; set; } = string.Empty;
        public string concilia_tipo { get; set; } = string.Empty;
        public DateTime concilia_fecha { get; set; }
        public string concilia_usuario { get; set; } = string.Empty;
        public string cod_plan { get; set; } = string.Empty;
        public string modo_protegido { get; set; } = string.Empty;

    }
}