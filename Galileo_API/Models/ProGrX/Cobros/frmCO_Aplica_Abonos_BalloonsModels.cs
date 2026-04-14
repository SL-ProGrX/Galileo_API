namespace Galileo_API.Models.ProGrX.Cobros
{
    public class CoAplicaAbonosBalloonsListaItemDto
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string codigo { get; set; } = string.Empty;
        public decimal operacion { get; set; }
        public decimal cuota { get; set; }
        public string preanalisis { get; set; } = string.Empty;
        public string periodicidad { get; set; } = string.Empty;
        public decimal disponible_cuenta { get; set; }
        public decimal disponible_sobres { get; set; }
        public decimal disponible_fondos { get; set; }
        public decimal disponible_fondos_especial { get; set; }
        public bool indicador { get; set; }
        public bool traslado_salario { get; set; }
    }

    public class CoAplicaAbonosBalloonsListaResult
    {
        public int total { get; set; }
        public List<CoAplicaAbonosBalloonsListaItemDto> lista { get; set; } = new();
    }

    public class CoAplicaAbonosBalloonsCasoAplicarDto
    {
        public string cedula { get; set; } = string.Empty;
        public decimal operacion { get; set; }
    }

    public class CoAplicaAbonosBalloonsAplicarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public List<CoAplicaAbonosBalloonsCasoAplicarDto> casos { get; set; } = new();
    }

    public class CoAplicaAbonosBalloonsAplicarDetalleDto
    {
        public string cedula { get; set; } = string.Empty;
        public decimal operacion { get; set; }
        public bool ok { get; set; }
        public string mensaje { get; set; } = string.Empty;
    }

    public class CoAplicaAbonosBalloonsAplicarResult
    {
        public int id_aplicacion { get; set; }
        public int procesados { get; set; }
        public int fallidos { get; set; }
        public List<CoAplicaAbonosBalloonsAplicarDetalleDto> detalle { get; set; } = new();
    }

    internal sealed class CoAplicaAbonosBalloonsGuiaDto
    {
        public int Aplicacion { get; set; }
    }
}