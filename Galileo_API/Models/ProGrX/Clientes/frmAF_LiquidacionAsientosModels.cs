using System.Text.Json.Serialization;

namespace Galileo.Models.ProGrX.Clientes
{

   

    public class FiltrosSolicitud
    {
        [JsonPropertyName("chkTodos")]
        public bool chkTodos { get; set; } = false;

        [JsonPropertyName("accion")]
        public string? accion { get; set; }

        [JsonPropertyName("estado")]
        public string? estado { get; set; }

        [JsonPropertyName("tipoRenuncia")]
        public string? tipoRenuncia { get; set; }

        [JsonPropertyName("tipo")]
        public string? tipo { get; set; }

        [JsonPropertyName("token")]
        public string? token { get; set; }

        [JsonPropertyName("fechaInicio")]
        public DateTime? fechaInicio { get; set; }

        [JsonPropertyName("fechaFin")]
        public DateTime? fechaFin { get; set; }
        public bool chkFiltros { get; set; } = false;
        public int? id_banco { get; set; }
        public string? cod_oficina { get; set; }
        public string? usuario { get; set; }
        public string? id_token { get; set; }
    }

    public class TokenConsultaModel
    {
        public string id_token { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string registro_usuario { get; set; } = string.Empty;
        public string itmx { get; set; } = string.Empty;
        public string idx { get; set; } = string.Empty;
        public DateTime registro_fecha { get; set; }
    }

    public class LiquidacionAsientoModel
    {
        public required decimal valor { get; set; }
        public required int consec { get; set; }
        public string? cedula { get; set; }
        public string? nombre { get; set; }
        public required decimal tneto { get; set; }
        public required int cod_banco { get; set; }
        public string? tdocumento { get; set; }
        public string? tipo { get; set; }
        public string? cuenta { get; set; }
        public required DateTime fecliq { get; set; }
        public string? usuario { get; set; }
        public string? descripcion { get; set; }
        public required int duplicado { get; set; }
        public DateTime? tes_supervision_fecha { get; set; }
        public string? cod_divisa { get; set; }
        public string? id_token { get; set; }
    }

    public class AfLiquidacionFiltroRequest
    {
        public DateTime desde { get; set; }
        public DateTime hasta { get; set; }

        public string? tipo_salida { get; set; }

        public string? estado_asiento { get; set; }

        public string? tipo_renuncia { get; set; }
    }

    public class AfLiquidacionAsientosBuscarRequest: AfLiquidacionFiltroRequest
    {
        // Equivalente a chkTodos.Value (en VB era -1/0). Aquí lo manejamos como bool.
        public bool marcar { get; set; } = false;

        // Filtros adicionales (solo si el front los activa)
        public int? filtro_banco { get; set; }
        public string? filtro_oficina { get; set; }
        public string? filtro_usuario { get; set; }
        public string? filtro_token { get; set; }
    }

    public class AfLiquidacionAsientosRowDto
    {
        // El SP devuelve "Valor" (en VB lo usaban para checkbox)
        public int valor { get; set; } = 0;

        public long consec { get; set; } = 0;
        public string? cedula { get; set; }
        public string? nombre { get; set; }

        public decimal tneto { get; set; } = 0;

        public int? cod_banco { get; set; }
        public string? tdocumento { get; set; }
        public string? tipo { get; set; }

        public string? cuenta { get; set; }
        public DateTime? fecliq { get; set; }

        public string? usuario { get; set; }
        public string? descripcion { get; set; }

        public int duplicado { get; set; } = 0;
        public DateTime? tes_supervision_fecha { get; set; }

        public string? cod_divisa { get; set; }
        public string? id_token { get; set; }
    }


    /// <summary>
    /// Request para generar traslado a Tesorería (equivalente a sbGenerar VB6).
    /// </summary>
    public class AfLiquidacionAsientosGenerarRequest
    {
        /// <summary>
        /// 'D' = Desembolsar (ejecuta SP Tesorería)
        /// 'R' = Retener (UPDATE directo en Liquidacion)
        /// </summary>
        public string accion { get; set; } = "D";

        /// <summary>
        /// Token seleccionado (VB6: cboToken.ItemData(cboToken.ListIndex))
        /// </summary>
        public string token { get; set; } = "";

        /// <summary>
        /// Usuario que ejecuta el proceso (normalmente viene del backend por sesión,
        /// pero se permite pasar si tu arquitectura lo usa así).
        /// </summary>
        public string usuario { get; set; } = "";

        /// <summary>
        /// Lista de liquidaciones a procesar. En VB6 esto venía de filas marcadas del grid.
        /// </summary>
        public List<AfLiquidacionAsientosGenerarItem> items { get; set; } = new();
    }

    public class AfLiquidacionAsientosGenerarItem
    {
        public long consec { get; set; }

        /// <summary>
        /// En VB6: columna 13 (Duplicado). Si es 1, NO se procesa.
        /// </summary>
        public int duplicado { get; set; } = 0;

        /// <summary>
        /// En VB6: columna 1 checkbox (Value = vbChecked).
        /// </summary>
        public bool marcado { get; set; } = true;
    }

    public class AfLiquidacionAsientosGenerarResponse
    {
        public int total_items { get; set; }
        public int seleccionados { get; set; }
        public int procesados { get; set; }
        public int omitidos_duplicado { get; set; }
        public int omitidos_no_marcado { get; set; }
    }
}