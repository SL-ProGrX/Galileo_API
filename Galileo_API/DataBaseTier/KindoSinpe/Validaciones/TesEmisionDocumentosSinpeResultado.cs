using Galileo.Models.ERROR;

namespace Galileo_API.DataBaseTier
{
    /// <summary>
    /// Normaliza los resultados de rechazo del servicio SINPE para que los
    /// consumidores no los interpreten como emisiones exitosas.
    /// </summary>
    public static class TesEmisionDocumentosSinpeResultado
    {
        public static ErrorDto
            TES_EmisionDocumentos_Sinpe_CrearRechazo(
                int codigo,
                string? mensaje)
        {
            return new ErrorDto
            {
                Code = codigo == 0 ? -1 : codigo,
                Description = string.IsNullOrWhiteSpace(mensaje)
                    ? "El servicio SINPE rechazó la solicitud."
                    : mensaje
            };
        }
    }
}
