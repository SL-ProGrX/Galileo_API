using System.Text.Json;
using System.Text.Json.Serialization;

namespace Galileo_API.Models.ProGrX_EstudioCrd
{
    public sealed class PreaScoreCrediticioRequest
    {
        [JsonRequired]
        public string cod_preanalisis { get; set; } = string.Empty;
        [JsonRequired]
        public string cedula { get; set; } = string.Empty;
    }

    internal sealed class CreditForceRespuesta
    {
        [JsonRequired]
        public int Tipo { get; set; }
        [JsonRequired]
        public string Detalle { get; set; } = string.Empty;
        [JsonRequired]
        public string TipoRespuesta { get; set; } = string.Empty;
    }

    internal sealed class CreditForceExisteResponse
    {
        [JsonRequired]
        public CreditForceRespuesta Respuesta { get; set; } = new();
        public CreditForceExisteResultado? ResultadoClienteExiste { get; set; }
    }

    internal sealed class CreditForceExisteResultado
    {
        [JsonRequired]
        public bool Existe { get; set; }
        [JsonRequired]
        public string NumeroIdentificacion { get; set; } = string.Empty;
        [JsonRequired]
        public bool TieneError { get; set; }
        public JsonElement? Error { get; set; }
    }

    internal sealed class CreditForceScoreResponse
    {
        [JsonRequired]
        public CreditForceRespuesta Respuesta { get; set; } = new();
        public CreditForceScoreResultado? ResultadoScoreClienteExiste { get; set; }
    }

    internal sealed class CreditForceScoreResultado
    {
        [JsonRequired]
        public string NumeroIdentificacion { get; set; } = string.Empty;
        [JsonRequired]
        public bool ClienteExisteEnComportamiento { get; set; }
        [JsonRequired]
        public string TipoClienteEvaluado { get; set; } = string.Empty;
        [JsonRequired]
        public bool TieneError { get; set; }
        public JsonElement? Error { get; set; }
        public CreditForceCalificaciones? Calificaciones { get; set; }
    }

    internal sealed class CreditForceCalificaciones
    {
        public CreditForceComportamiento? Comportamiento { get; set; }
    }

    internal sealed class CreditForceComportamiento
    {
        [JsonRequired]
        public string Modelo { get; set; } = string.Empty;
        public string? Objetivo { get; set; }
        [JsonRequired]
        public decimal ProbabilidadDefecto { get; set; }
        [JsonRequired]
        public int DecisionBinaria { get; set; }
        [JsonRequired]
        public string Veredicto { get; set; } = string.Empty;
        [JsonRequired]
        public string CategoriaRiesgo { get; set; } = string.Empty;
        [JsonRequired]
        public decimal UmbralAplicado { get; set; }
    }
}
