namespace Galileo.Models.INV
{
    public sealed class InvVentasEnLineaSincRequest
    {
        public string? usuario { get; set; }
    }

    public sealed class InvVentasEnLineaSincData
    {
        public string proceso { get; set; } = string.Empty;

        public string mensaje { get; set; } = string.Empty;
    }
}