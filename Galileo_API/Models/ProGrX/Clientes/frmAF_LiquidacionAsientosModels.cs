using System.Text.Json.Serialization;

namespace PgxAPI.Models.ProGrX.Clientes
{
    public class FiltrosSolicitud
    {
        [JsonPropertyName("chkTodos")]
        public bool chkTodos { get; set; } = false;

        [JsonPropertyName("accion")]
        public string accion { get; set; }

        [JsonPropertyName("estado")]
        public string estado { get; set; }

        [JsonPropertyName("tipoRenuncia")]
        public string tipoRenuncia { get; set; }

        [JsonPropertyName("tipo")]
        public string tipo { get; set; }

        [JsonPropertyName("token")]
        public string token { get; set; }

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
        public decimal valor { get; set; }
        public int consec { get; set; }
        public string cedula { get; set; }
        public string nombre { get; set; }
        public decimal tneto { get; set; }
        public int cod_banco { get; set; }
        public string tdocumento { get; set; }
        public string tipo { get; set; }
        public string cuenta { get; set; }
        public DateTime fecliq { get; set; }
        public string usuario { get; set; }
        public string descripcion { get; set; }
        public int duplicado { get; set; }
        public DateTime? tes_supervision_fecha { get; set; }
        public string cod_divisa { get; set; }
        public string? id_token { get; set; }
    }
}
