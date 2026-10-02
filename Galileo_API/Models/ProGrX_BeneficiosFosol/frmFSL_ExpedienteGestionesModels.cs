using System.Text.Json.Serialization;

namespace Galileo.Models.FSL
{
    public class FslExpedienteGestionAgregarRequest
    {
        [JsonRequired]
        public long cod_expediente { get; set; } = 0;

        [JsonRequired]
        public string cod_gestion { get; set; } = string.Empty;

        public string? notas { get; set; } = string.Empty;

        [JsonRequired]
        public string usuario { get; set; } = string.Empty;
    }
}