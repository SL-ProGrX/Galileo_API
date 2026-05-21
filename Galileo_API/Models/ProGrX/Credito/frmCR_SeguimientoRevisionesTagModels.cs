namespace Galileo_API.Models.ProGrX.Credito
{
    namespace Galileo_API.Models.ProGrX.Credito
    {
        #region Operaciones

        public class CrSeguimientoRevisionesTagOperacionesFiltrosRequest
        {
            public string etiqueta_filtro { get; set; } = string.Empty;
            public bool solo_creditos_espera { get; set; } = false;
            public List<string> bancos { get; set; } = new();
            public int? id_solicitud { get; set; }
        }

        public class CrSeguimientoRevisionesTagOperacionRow
        {
            public long id_solicitud { get; set; } = 0;
            public string cedula { get; set; } = string.Empty;
            public string nombre { get; set; } = string.Empty;
            public string codigo { get; set; } = string.Empty;
            public decimal montosol { get; set; } = 0;
            public decimal cuota { get; set; } = 0;
            public int plazo { get; set; } = 0;
            public decimal @int { get; set; } = 0;
            public string estadosol { get; set; } = string.Empty;
            public DateTime? fechasol { get; set; }
            public string remesa { get; set; } = string.Empty;
            public string usuario_remesa { get; set; } = string.Empty;
        }

        public class CrSeguimientoRevisionesTagOperacionesResponse
        {
            public int total { get; set; } = 0;
            public List<CrSeguimientoRevisionesTagOperacionRow> lista { get; set; } = new();
        }

        #endregion

        #region Catalogos

        public class CrSeguimientoRevisionesTagBancoRow
        {
            public string id_banco { get; set; } = string.Empty;
            public string descripcion { get; set; } = string.Empty;
        }

        public class CrSeguimientoRevisionesTagEtiquetaRow
        {
            public string idx { get; set; } = string.Empty;
            public string descripcion { get; set; } = string.Empty;
        }

        public class CrSeguimientoRevisionesTagNotaLargoResponse
        {
            public int nota_largo { get; set; } = 0;
        }

        public class CrSeguimientoRevisionesTagAvisoResponse
        {
            public string mensaje { get; set; } = string.Empty;
        }

        #endregion

        #region Detalle

        public class CrSeguimientoRevisionesTagDetalleRequest
        {
            public long id_solicitud { get; set; } = 0;
        }

        public class CrSeguimientoRevisionesTagDetalleCreditoResponse
        {
            public string cedula { get; set; } = string.Empty;
            public string nombre { get; set; } = string.Empty;
            public long id_solicitud { get; set; } = 0;
            public string garantia { get; set; } = string.Empty;
            public decimal montosol { get; set; } = 0;
            public decimal montoapr { get; set; } = 0;
            public decimal cuota { get; set; } = 0;
            public decimal monto_girado { get; set; } = 0;
            public decimal montodesembolsos { get; set; } = 0;
            public decimal montorefundicion { get; set; } = 0;
            public decimal refundicionescuota { get; set; } = 0;
            public decimal desembolsoscuota { get; set; } = 0;
            public decimal total_cuotas { get; set; } = 0;
            public decimal diferencia_cuota { get; set; } = 0;
        }

        public class CrSeguimientoRevisionesTagFiadorRequest
        {
            public string cedula { get; set; } = string.Empty;
            public string tipo { get; set; } = string.Empty;
            public long id_solicitud { get; set; } = 0;
        }

        public class CrSeguimientoRevisionesTagFiadorResponse
        {
            public string membresia { get; set; } = string.Empty;
            public DateTime? fechaingreso { get; set; }
            public string institucion { get; set; } = string.Empty;
            public decimal salario_liquido { get; set; } = 0;
            public decimal liquidez_simple { get; set; } = 0;
            public decimal liquidez_cfianzas { get; set; } = 0;
            public decimal liquidez_simple_porc { get; set; } = 0;
            public decimal liquidez_cfianzas_porc { get; set; } = 0;
            public string provincia { get; set; } = string.Empty;
            public string canton { get; set; } = string.Empty;
            public string distrito { get; set; } = string.Empty;
            public string direccion { get; set; } = string.Empty;
            public string cod_preanalisis { get; set; } = string.Empty;
        }

        public class CrSeguimientoRevisionesTagClasificacionRow
        {
            public string descripcion { get; set; } = string.Empty;
            public string valor { get; set; } = string.Empty;
            public string observacion { get; set; } = string.Empty;
        }

        public class CrSeguimientoRevisionesTagDeudaRow
        {
            public bool seleccionado { get; set; } = false;
            public string semaforo { get; set; } = string.Empty;
            public string operacion { get; set; } = string.Empty;
            public string linea { get; set; } = string.Empty;
            public decimal plazo { get; set; } = 0;
            public decimal monto { get; set; } = 0;
            public decimal saldo { get; set; } = 0;
            public decimal cuota { get; set; } = 0;
            public string primero { get; set; } = string.Empty;
            public decimal mora { get; set; } = 0;
            public string garantia { get; set; } = string.Empty;
        }

        public class CrSeguimientoRevisionesTagDeudasResponse
        {
            public decimal total_saldo { get; set; } = 0;
            public decimal total_cuota { get; set; } = 0;
            public decimal deducciones { get; set; } = 0;
            public List<CrSeguimientoRevisionesTagDeudaRow> lista { get; set; } = new();
        }

        public class CrSeguimientoRevisionesTagFianzaRow
        {
            public string operacion { get; set; } = string.Empty;
            public string linea { get; set; } = string.Empty;
            public string fiador { get; set; } = string.Empty;
            public decimal monto { get; set; } = 0;
            public decimal saldo { get; set; } = 0;
            public decimal cuota { get; set; } = 0;

            public string cedula_deudor { get; set; } = string.Empty;
            public string nombre { get; set; } = string.Empty;
        }

        public class CrSeguimientoRevisionesTagFianzasResponse
        {
            public decimal monto { get; set; } = 0;
            public decimal saldo { get; set; } = 0;
            public decimal cuota { get; set; } = 0;
            public List<CrSeguimientoRevisionesTagFianzaRow> lista { get; set; } = new();
        }

        public class CrSeguimientoRevisionesTagRefundicionRow
        {
            public string operacion { get; set; } = string.Empty;
            public string linea { get; set; } = string.Empty;
            public decimal plazo { get; set; } = 0;
            public decimal monto { get; set; } = 0;
            public decimal refundicion { get; set; } = 0;
            public decimal cuota { get; set; } = 0;
            public string garantia { get; set; } = string.Empty;
        }

        public class CrSeguimientoRevisionesTagDesembolsoRow
        {
            public string concepto { get; set; } = string.Empty;
            public decimal monto { get; set; } = 0;
            public decimal cuota { get; set; } = 0;
        }

        public class CrSeguimientoRevisionesTagPatrimonioResponse
        {
            public decimal aporte_obrero { get; set; } = 0;
            public decimal patronal { get; set; } = 0;
            public decimal capitalizacion { get; set; } = 0;
            public decimal ahorros_extra { get; set; } = 0;
            public DateTime? fecha_corte { get; set; }
            public decimal ahorros_fecha { get; set; } = 0;
            public decimal saldo_prestamos { get; set; } = 0;
            public decimal disponible_bruto { get; set; } = 0;
            public decimal total { get; set; } = 0;
            public decimal disponible { get; set; } = 0;
        }

        public class CrSeguimientoRevisionesTagDetalleResponse
        {
            public CrSeguimientoRevisionesTagDetalleCreditoResponse credito { get; set; } = new();
            public CrSeguimientoRevisionesTagPatrimonioResponse patrimonio { get; set; } = new();
            public List<CrSeguimientoRevisionesTagDeudaRow> deudas { get; set; } = new();
            public List<CrSeguimientoRevisionesTagFianzaRow> fianzas { get; set; } = new();
            public List<CrSeguimientoRevisionesTagRefundicionRow> refundiciones { get; set; } = new();
            public List<CrSeguimientoRevisionesTagDesembolsoRow> desembolsos { get; set; } = new();
        }

        public class CrSeguimientoRevisionesTagPersonaRow
        {
            public string cedula { get; set; } = string.Empty;
            public string nombre { get; set; } = string.Empty;
            public string estado { get; set; } = string.Empty;
            public string calidad { get; set; } = string.Empty;
            public string est_lab { get; set; } = string.Empty;
        }

        #endregion

        #region Seguimiento

        public class CrSeguimientoRevisionesTagSeguimientoRequest
        {
            public long id_solicitud { get; set; } = 0;
        }

        public class CrSeguimientoRevisionesTagSeguimientoRow
        {
            public string descripcion { get; set; } = string.Empty;
            public string notas { get; set; } = string.Empty;
            public DateTime? registro_fecha { get; set; }
            public string registro_usuario { get; set; } = string.Empty;
        }

        public class CrSeguimientoRevisionesTagSeguimientoResponse
        {
            public int total { get; set; } = 0;
            public List<CrSeguimientoRevisionesTagSeguimientoRow> lista { get; set; } = new();
        }

        #endregion

        #region Revision

        public class CrSeguimientoRevisionesTagErrorRow
        {
            public int linea { get; set; } = 0;
            public bool seleccionado { get; set; } = false;
            public string error_codigo { get; set; } = string.Empty;
            public string error_descripcion { get; set; } = string.Empty;
            public string aplicado { get; set; } = string.Empty;
            public string nota_default { get; set; } = string.Empty;
        }

        public class CrSeguimientoRevisionesTagRevisionResponse
        {
            public string nombre_usuario { get; set; } = string.Empty;
            public string tag_revision { get; set; } = string.Empty;
            public bool operacion_revisada { get; set; } = false;
            public List<CrSeguimientoRevisionesTagErrorRow> errores { get; set; } = new();
        }

        public class CrSeguimientoRevisionesTagAplicarRequest
        {
            public long id_solicitud { get; set; } = 0;
            public string tag_codigo { get; set; } = string.Empty;
            public string observacion { get; set; } = string.Empty;
            public List<int> errores_seleccionados { get; set; } = new();
        }

        public class CrSeguimientoRevisionesTagAplicarResponse
        {
            public bool aplicado { get; set; } = false;
            public bool analistas_revision { get; set; } = false;
            public string mensaje { get; set; } = string.Empty;
        }

        #endregion

      

    }
}
