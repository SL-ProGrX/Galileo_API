namespace Galileo_API.Models.ProGrX_Polizas
{
    public class FrmCrPolizaProcRecepcionModels
    {
        /// <summary>
        /// Resultado estandar de los SP del corte (Pass, Mensaje, Movimiento, idFactura).
        /// Nombres homologados al recordset VB6 (Regla 12).
        /// </summary>
        public class PolizaAseguradoraCorte
        {
            public int? pass { get; set; }
            public string mensaje { get; set; } = string.Empty;
            public string movimiento { get; set; } = string.Empty;
            public int? idfactura { get; set; }
        }

        /// <summary>
        /// Datos del encabezado de la factura (spPoliza_Aseguradora_Corte_Add).
        /// </summary>
        public class PolizaAseguradoraCorteData
        {
            public DateTime? corte { get; set; }
            public string cod_poliza { get; set; } = string.Empty;
            public int? id_factura { get; set; }
            public string cod_aseguradora { get; set; } = string.Empty;
            public long? cod_proveedor { get; set; }
            public string factura { get; set; } = string.Empty;
            public string forma_pago { get; set; } = string.Empty;
            public DateTime? vence { get; set; }
            public string cod_divisa { get; set; } = string.Empty;
            public string cod_unidad { get; set; } = string.Empty;
            public string cod_centro_costo { get; set; } = string.Empty;
            public decimal? tipo_cambio { get; set; }
            public string notas { get; set; } = string.Empty;
        }

        /// <summary>
        /// Fila del archivo Excel (hoja IMPORT): CEDULA, NOMBRE, N_POLIZA, MONTO_ASEGURADO, PRIMA, OPERACION.
        /// </summary>
        public class PolizaAseguradoraCorteDetalleData
        {
            public string cedula { get; set; } = string.Empty;
            public string nombre { get; set; } = string.Empty;
            public string n_poliza { get; set; } = string.Empty;
            public decimal? monto_asegurado { get; set; }
            public decimal? prima { get; set; }
            public long? operacion { get; set; }
        }

        /// <summary>
        /// Request de carga del archivo: encabezado de la factura mas filas del Excel.
        /// </summary>
        public class PolizaArchivoCargarRequest
        {
            public PolizaAseguradoraCorteData? datos { get; set; }
            public List<PolizaAseguradoraCorteDetalleData> filas { get; set; } = new();
        }

        /// <summary>
        /// Request para eliminar los registros seleccionados del detalle del corte.
        /// </summary>
        public class PolizaDetalleEliminarRequest
        {
            public DateTime? corte { get; set; }
            public string cod_poliza { get; set; } = string.Empty;
            public int? id_factura { get; set; }
            public List<long> registros { get; set; } = new();
        }

        /// <summary>
        /// Fila del detalle del corte (spPoliza_Aseguradora_Corte_Detalle_Consulta).
        /// </summary>
        public class PolizaAseguradoraCorteDetalleConsulta
        {
            public long? id_registro { get; set; }
            public long? id_solicitud { get; set; }
            public string n_poliza { get; set; } = string.Empty;
            public string cedula { get; set; } = string.Empty;
            public string nombre { get; set; } = string.Empty;
            public decimal? monto_asegurado { get; set; }
            public decimal? prima { get; set; }
        }

        /// <summary>
        /// Datos de la poliza (spPoliza_Poliza_Datos).
        /// </summary>
        public class PolizaDatos
        {
            public object? id_factura { get; set; }
            public object? cod_aseguradora { get; set; }
            public string aseguradora_desc { get; set; } = string.Empty;
            public object? cod_proveedor { get; set; }
            public string proveedor_desc { get; set; } = string.Empty;
            public object? cod_unidad { get; set; }
            public string unidad_desc { get; set; } = string.Empty;
            public object? cod_centro_costo { get; set; }
            public string centro_costo_desc { get; set; } = string.Empty;
            public DateTime? corte { get; set; }
        }

        /// <summary>
        /// Corte ya cargado (spPoliza_Aseguradora_Corte_Consulta). Reutiliza PolizaDatos (Regla 6).
        /// </summary>
        public class PolizaAseguradoraCorteConsulta : PolizaDatos
        {
            public string factura { get; set; } = string.Empty;
            public string notas { get; set; } = string.Empty;
            public DateTime? vence { get; set; }
            public DateTime? cxp_fecha { get; set; }
            public string forma_pago_desc { get; set; } = string.Empty;
        }
    }
}
