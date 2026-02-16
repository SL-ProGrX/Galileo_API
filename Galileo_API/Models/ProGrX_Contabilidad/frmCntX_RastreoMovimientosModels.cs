namespace Galileo_API.Models.ProGrX_Contabilidad
{
    public class RastreoMovimientosFiltroDto
    {
        public string? tipo { get; set; }
        public int? codigo { get; set; }

        public DateTime? fechaInicio { get; set; }
        public DateTime? fechaCorte { get; set; }

        public string? cuentaInicio { get; set; }
        public string? cuentaCorte { get; set; }

        public string? movimiento { get; set; }
        public string? signo { get; set; }
        public decimal? parametro { get; set; }

        public string? documento { get; set; }
        public string? detalle { get; set; }
    }

    public class RastreoMovimientosTablaDto
    {
        public string cuenta { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public DateTime fecha { get; set; }
        public string tipo_asiento { get; set; } = string.Empty;
        public string num_asiento { get; set; } = string.Empty;
        public decimal debitos { get; set; }
        public decimal creditos { get; set; }
        public string empresa { get; set; } = string.Empty;
        public string documento { get; set; } = string.Empty;
        public string detalle { get; set; } = string.Empty;
    }



}
