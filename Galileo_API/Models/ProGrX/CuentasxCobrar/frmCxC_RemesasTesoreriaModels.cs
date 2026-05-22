namespace Galileo_API.Models.ProGrX.CuentasxCobrar
{
    public static class CxCRemesasTesoreriaConstantes
    {
        public const string EstadoAbierta = "A";
        public const string EstadoCerrada = "C";
        public const string EstadoTrasladada = "T";
        public const string Operacion = "Operacion";
        public const string EstadoAbiertaDesc = "Abierta";
        public const string EstadoCerradaDesc = "Cerrada";
        public const string EstadoTrasladadaDesc = "Trasladada";

        public const string TipoCheque = "CK";
        public const string TipoTransferencia = "TE";

        public const string ParametroUnidadOmision = "05";
        public const string ParametroConceptoOmision = "06";

        public const string MensajeOk = "Ok";
        public const string MensajeRemesaRequerida = "La remesa es requerida.";
        public const string MensajeUsuarioRequerido = "El usuario es requerido.";
        public const string MensajeOperacionRequerida = "La operación es requerida.";
        public const string MensajeRemesaNoExiste = "No se encontró la remesa.";
        public const string MensajeRemesaNoAbierta = "La Remesa actual; ya se encuentra cerrada...";
        public const string MensajeRemesaNoCerrada = "La remesa debe estar cerrada para realizar el traslado.";
        public const string MensajeOperacionNoExiste = "La Operacion Digitada no existe...";
        public const string MensajeOperacionNoReactivable = "La operación no puede ser reactivada.";
        public const string MensajeCargaOk = "Proceso Realizado Satisfactoriamente...";
        public const string MensajeCerrarOk = "Remesa Cerrada Satisfactoriamente...";
        public const string MensajeTrasladoOk = "Operaciones Enviadas a Tesoreria Satisfactoriamente...";
        public const string MensajeReactivacionOk = "Operación ReActivada Satisfactoriamente...";
        public const string MensajeFechasRemesaInvalidas = "No fue posible obtener las fechas de la remesa.";
        public const string BitacoraRegistra = "Registra";
        public const string BitacoraModifica = "Modifica";
        public const string BitacoraElimina = "Elimina";
        public const string BitacoraAplica = "Aplica";
        public const string MensajeFiltrosInvalidos = "No fue posible procesar los filtros.";
        public const string PaginacionSql = " OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY;";
        public const string MensajeEliminarOk = "Remesa eliminada correctamente.";
        public const string MensajeFechasRemesaRequeridas = "Las fechas de la remesa son requeridas.";
        public const string MensajeSeleccioneOperacion = "Debe seleccionar al menos una operación.";
        public const string MensajeValidacionFallida = "No fue posible validar la solicitud.";
    }

    public class CxCRemesasTesoreriaRemesaLista
    {
        public int total { get; set; }
        public List<CxCRemesasTesoreriaRemesaData> lista { get; set; } = new();
    }

    public class CxCRemesasTesoreriaRemesaData
    {
        public int tesoreria_remesa { get; set; }
        public string usuario { get; set; } = string.Empty;
        public DateTime? fecha { get; set; }
        public string estado { get; set; } = string.Empty;
        public string estado_desc { get; set; } = string.Empty;
        public DateTime? fecha_inicio { get; set; }
        public DateTime? fecha_corte { get; set; }
        public int casos { get; set; }
        public decimal monto { get; set; }
        public string notas { get; set; } = string.Empty;
    }

    public class CxCRemesasTesoreriaRemesaGuardarRequest
    {
        public int? tesoreria_remesa { get; set; }
        public DateTime? fecha_inicio { get; set; }
        public DateTime? fecha_corte { get; set; }
        public string notas { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
    }

    public class CxCRemesasTesoreriaOperacionLista
    {
        public int total { get; set; }
        public decimal total_monto { get; set; }
        public List<CxCRemesasTesoreriaOperacionData> lista { get; set; } = new();
    }

    public class CxCRemesasTesoreriaOperacionData
    {
        public int operacion { get; set; }
        public string cod_concepto { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public decimal desembolso_monto { get; set; }
        public decimal desembolsos { get; set; }
        public decimal otros_giros { get; set; }
        public decimal total { get; set; }
        public bool seleccionado { get; set; }
    }

    public class CxCRemesasTesoreriaCargaAplicarRequest
    {
        public int? tesoreria_remesa { get; set; }
        public string usuario { get; set; } = string.Empty;
        public List<int> operaciones { get; set; } = new();
    }

    public class CxCRemesasTesoreriaCerrarRequest
    {
        public int? tesoreria_remesa { get; set; }
        public string usuario { get; set; } = string.Empty;
    }

    public class CxCRemesasTesoreriaTrasladoAplicarRequest
    {
        public int? tesoreria_remesa { get; set; }
        public string usuario { get; set; } = string.Empty;
        public bool? agrupar { get; set; }
    }

    public class CxCRemesasTesoreriaReactivacionDto
    {
        public int operacion { get; set; }
        public string cod_concepto { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public decimal desembolso_monto { get; set; }
        public long? tesoreria_solicitud { get; set; }
        public bool puede_reactivar { get; set; }
        public string detalle { get; set; } = string.Empty;
        public string documento { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public int? id_banco { get; set; }
    }

    public class CxCRemesasTesoreriaReactivacionAplicarRequest
    {
        public int? operacion { get; set; }
        public string usuario { get; set; } = string.Empty;
    }

    public class CxCRemesasTesoreriaTrasladoRow
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string emitir_tipo { get; set; } = string.Empty;
        public int emitir_banco { get; set; }
        public decimal desembolso_monto { get; set; }
        public string emitir_cuenta { get; set; } = string.Empty;
        public string cod_unidad { get; set; } = string.Empty;
        public string cod_centro_costo { get; set; } = string.Empty;
        public string bancocta { get; set; } = string.Empty;
        public string cod_concepto { get; set; } = string.Empty;
        public string num_documento { get; set; } = string.Empty;
        public string operacion { get; set; } = string.Empty;
        public string conceptocta { get; set; } = string.Empty;
        public string cod_contrato { get; set; } = string.Empty;
    }

    public class CxCRemesasTesoreriaTrasladoDetalleRow
    {
        public string cod_concepto { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string emitir_tipo { get; set; } = string.Empty;
        public int emitir_banco { get; set; }
        public decimal desembolso_monto { get; set; }
        public string emitir_cuenta { get; set; } = string.Empty;
        public string cod_unidad { get; set; } = string.Empty;
        public string cod_centro_costo { get; set; } = string.Empty;
        public string cod_contrato { get; set; } = string.Empty;
        public string conceptocta { get; set; } = string.Empty;
        public string bancocta { get; set; } = string.Empty;
        public string num_documento { get; set; } = string.Empty;
        public string operacion { get; set; } = string.Empty;
    }

    public class CxCRemesasTesoreriaMaestroRequest
    {
        public string cedula { get; set; } = string.Empty;
        public string tipo_documento { get; set; } = string.Empty;
        public int banco { get; set; }
        public decimal monto { get; set; }
        public string codigo { get; set; } = string.Empty;
        public string beneficiario { get; set; } = string.Empty;
        public string detalle1 { get; set; } = string.Empty;
        public string detalle2 { get; set; } = string.Empty;
        public string cuenta { get; set; } = string.Empty;
        public DateTime fecha { get; set; }
        public string unidad_omision { get; set; } = string.Empty;
        public string concepto_omision { get; set; } = string.Empty;
        public string usuario_solicita { get; set; } = string.Empty;
        public decimal tipo_cambio { get; set; } = 1;
        public string divisa { get; set; } = "COL";
        public int op { get; set; }
        public long referencia { get; set; }
    }
}