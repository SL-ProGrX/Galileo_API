namespace Galileo_API.Models
{
    public sealed class MValidacionDestino
    {
        public MValidacionDestino(
            string cod_destino,
            string descripcion,
            string campo)
        {
            this.cod_destino = cod_destino;
            this.descripcion = descripcion;
            this.campo = campo;
        }

        public string cod_destino { get; }

        public string descripcion { get; }

        public string campo { get; }
    }

    public sealed class MValidacionBonoMembresiaRequest
    {
        public string cedula { get; set; } = string.Empty;

        public string linea { get; set; } = string.Empty;

        public string garantia { get; set; } = string.Empty;

        public string destino { get; set; } = string.Empty;

        public int plazo { get; set; } = 1;
    }

    public sealed class MValidacionCatalogoRangoRequest
    {
        public string codigo { get; set; } = string.Empty;

        public decimal monto { get; set; } = 0;

        public string tipo { get; set; } = string.Empty;

        public string cod_destino { get; set; } = string.Empty;

        public string garantia { get; set; } = string.Empty;
    }

    public sealed class MValidacionCatalogoRangoPlazoRequest
    {
        public string codigo { get; set; } = string.Empty;

        public int plazo { get; set; } = 0;

        public string cod_destino { get; set; } = string.Empty;

        public string garantia { get; set; } = string.Empty;
    }

    public sealed class MValidacionCuotaRequest
    {
        public decimal monto { get; set; } = 0;

        public int plazo { get; set; } = 0;

        public decimal interes { get; set; } = 0;

        public string frecuencia { get; set; } = "M";
    }
}