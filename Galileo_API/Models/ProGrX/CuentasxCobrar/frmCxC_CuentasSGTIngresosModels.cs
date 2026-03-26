namespace Galileo_API.Models.ProGrX.CuentasxCobrar
{
    public class CxCIngresoDto
    {
        public int operacion { get; set; }
        public int linea { get; set; }

        public DateTime? fecha { get; set; }

        public decimal? monto { get; set; }

        public string? descripcion { get; set; }

        public string? usuario { get; set; }

        public DateTime? fecha_registro { get; set; }
    }

    public class CxCIngresoGuardarDto
    {
        public int? operacion { get; set; }

        public int? linea { get; set; } // null = insert

        public string cod_cargo { get; set; } = string.Empty;

        public string tipo { get; set; } = string.Empty; // "M" o "P"

        public decimal? valor { get; set; }

        public decimal? monto { get; set; }

        public string? detalle { get; set; }

        public string usuario { get; set; } = string.Empty;
    }
}
