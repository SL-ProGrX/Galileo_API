using System.Text.Json.Serialization;

namespace Galileo.Models.FSL
{
    public class FslExpedienteApelacionAgregarRequest
    {
        [JsonRequired]
        public long cod_expediente { get; set; } = 0;

        public string cod_apelacion { get; set; } =
            string.Empty;

        public string presenta_identificacion { get; set; } =
            string.Empty;

        public string presenta_nombre { get; set; } =
            string.Empty;

        public string notas { get; set; } =
            string.Empty;

        public string usuario { get; set; } =
            string.Empty;
    }

    public class FslExpedienteApelacionResolucionGuardarRequest
    {
        [JsonRequired]
        public long cod_expediente { get; set; } = 0;

        public string resolucion { get; set; } =
            string.Empty;

        public string resolucion_notas { get; set; } =
            string.Empty;

        public string resolucion_usuario { get; set; } =
            string.Empty;

        public List<FslExpedienteResolucionMiembroRequest>
            miembros
        { get; set; } = [];
    }
}