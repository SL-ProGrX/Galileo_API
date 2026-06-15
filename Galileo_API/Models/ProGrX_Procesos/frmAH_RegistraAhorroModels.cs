public class SifDocumentosDto
{
    public string idx { get; set; } = string.Empty;
    public string itmx { get; set; } = string.Empty;
}

public class TransaccionSifdto
{
    public string cod_transaccion { get; set; } = string.Empty;
    public string tipo_documento { get; set; } = string.Empty;
    public string cod_concepto { get; set; } = string.Empty;
    public string cliente_identificacion { get; set; } = string.Empty;
    public string documento { get; set; } = string.Empty;
    public string cliente_nombre { get; set; } = string.Empty;
    public string monto { get; set; } = string.Empty;
    public DateTime registro_fecha { get; set; }
    public string registro_usuario { get; set; } = string.Empty;
    public string estado { get; set; } = string.Empty;
    public string traspaso { get; set; } = string.Empty;
    public DateTime traspaso_fecha { get; set; }
    public string traspaso_usuario { get; set; } = string.Empty;
    public DateTime anulacion_fecha { get; set; }
    public string anulacion_usuario { get; set; } = string.Empty;
    public string linea1 { get; set; } = string.Empty;
    public string linea2 { get; set; } = string.Empty;
    public string linea3 { get; set; } = string.Empty;
    public string linea4 { get; set; } = string.Empty;
    public string linea5 { get; set; } = string.Empty;
    public string linea6 { get; set; } = string.Empty;
    public string linea7 { get; set; } = string.Empty;
    public string linea8 { get; set; } = string.Empty;
    public string linea9 { get; set; } = string.Empty;
    public string linea10 { get; set; } = string.Empty;
    public string detalle { get; set; } = string.Empty;
    public string modulo { get; set; } = string.Empty;
    public string referencia_01 { get; set; } = string.Empty;
    public string referencia_02 { get; set; } = string.Empty;
    public string referencia_03 { get; set; } = string.Empty;
    public string cod_apertura { get; set; } = string.Empty;
    public string cod_caja { get; set; } = string.Empty;
    public string cod_oficina { get; set; } = string.Empty;
    public string reintegro_monto { get; set; } = string.Empty;
    public DateTime reintegro_fecha { get; set; }
    public string reintegro_solicitud { get; set; } = string.Empty;
    public string linea11 { get; set; } = string.Empty;
    public string traslado_bloqueo { get; set; } = string.Empty;
    public string analista_revision { get; set; } = string.Empty;
    public string analista_recepcion { get; set; } = string.Empty;
    public string caja_am_id { get; set; } = string.Empty;
}

public class FrmAhRegistraAhorroCargarRequest
{
    public string cedula { get; set; } = string.Empty;
    public string usuario { get; set; } = string.Empty;
    public string cod_caja { get; set; } = string.Empty;
}

public class FrmAhRegistraAhorroRubroDto
{
    public string idx { get; set; } = string.Empty;
    public string itmx { get; set; } = string.Empty;
    public decimal aporte_autorizado { get; set; } = 0;
    public bool requiere_autorizacion { get; set; } = false;
}

public class FrmAhRegistraAhorroCargarResponse
{
    public string cedula { get; set; } = string.Empty;
    public string nombre { get; set; } = string.Empty;
    public string cod_divisa { get; set; } = string.Empty;
    public string estado_actual { get; set; } = string.Empty;
    public decimal aporte_manual { get; set; } = 0;
    public bool caja_valida_concepto { get; set; } = false;
    public string caja_validacion_mensaje { get; set; } = string.Empty;
    public string tiquete { get; set; } = string.Empty;
    public string tipo_rubro_default { get; set; } = string.Empty;
    public string fecha_proceso_default { get; set; } = string.Empty;
    public string tipo_documento_default { get; set; } = string.Empty;
    public decimal aporte_autorizado_default { get; set; } = 0;
    public List<FrmAhRegistraAhorroRubroDto> rubros { get; set; } = [];
    public List<SifDocumentosDto> procesos { get; set; } = [];
    public List<SifDocumentosDto> tipos_documento { get; set; } = [];
}

public class FrmAhRegistraAhorroGestionRegistrarRequest
{
    public string cedula { get; set; } = string.Empty;
    public string tipo { get; set; } = string.Empty;
    public decimal mnt_cal { get; set; } = 0;
    public decimal mnt_sol { get; set; } = 0;
    public string usuario { get; set; } = string.Empty;
}

public class FrmAhRegistraAhorroGestionResponse
{
    public int gestion_id { get; set; } = 0;
    public string gestion_estado { get; set; } = string.Empty;
}

public class FrmAhRegistraAhorroAplicarRequest
{
    public string cedula { get; set; } = string.Empty;
    public string nombre { get; set; } = string.Empty;
    public string usuario { get; set; } = string.Empty;
    public string cod_caja { get; set; } = string.Empty;
    public int apertura { get; set; } = 0;
    public int sesion_id { get; set; } = 0;
    public string tiquete { get; set; } = string.Empty;
    public string tipo_rubro { get; set; } = string.Empty;
    public string fecha_proceso { get; set; } = string.Empty;
    public string tipo_documento { get; set; } = string.Empty;
    public decimal aporte_autorizado { get; set; } = 0;
    public decimal monto { get; set; } = 0;
    public decimal total_cajas { get; set; } = 0;
    public string notas { get; set; } = string.Empty;
    public bool recibo_digital { get; set; } = false;
    public bool es_ajuste { get; set; } = false;
    public int gestion_id { get; set; } = 0;
    public string gestion_estado { get; set; } = string.Empty;
}

public class FrmAhRegistraAhorroAplicarResponse
{
    public string tipo_documento { get; set; } = string.Empty;
    public string num_documento { get; set; } = string.Empty;
    public decimal monto_aplicado { get; set; } = 0;
    public bool recibo_digital_enviado { get; set; } = false;
    public string advertencias { get; set; } = string.Empty;
    public string mensaje { get; set; } = string.Empty;
    public string? reporte_resultado { get; set; }
}

public class FrmAhRegistraAhorroSocioDto
{
    public string cedula { get; set; } = string.Empty;
    public string nombre { get; set; } = string.Empty;
    public string estado_actual { get; set; } = string.Empty;
    public string cod_divisa { get; set; } = string.Empty;
    public int caja_valida_concepto { get; set; } = 0;
    public decimal aporte_manual { get; set; } = 0;
}