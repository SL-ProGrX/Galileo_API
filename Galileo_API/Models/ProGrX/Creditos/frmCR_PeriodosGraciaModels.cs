namespace Galileo_API.Models.ProGrX.Creditos
{
    public class CrPeriodosGraciaConsultaRequest
    {
        public string? Linea { get; set; }
        public string? Garantia { get; set; }
        public string? Destino { get; set; }
        public string? Recurso { get; set; }
        public string? Institucion { get; set; }
        public string? Deductora { get; set; }
        public string? Divisa { get; set; }
        public string? EstadoPersona { get; set; }
        public string? EstadoLaboral { get; set; }

        public DateTime? FormalizaInicio { get; set; }
        public DateTime? FormalizaCorte { get; set; }
        public DateTime? AplInicio { get; set; }
        public DateTime? AplCorte { get; set; }

        public bool? PlazoRng { get; set; }
        public int? PlazoInicio { get; set; }
        public int? PlazoCorte { get; set; }

        public bool? TasaRng { get; set; }
        public decimal? TasaInicio { get; set; }
        public decimal? TasaCorte { get; set; }

        public string? CobroTipo { get; set; }
        public string? OperacionTipo { get; set; }

        public bool? PriDeducApl { get; set; }
        public string? PriDeducFiltro { get; set; }
        public int? PriDeduc { get; set; }

        public bool? UltDeducApl { get; set; }
        public string? UltDeducFiltro { get; set; }
        public int? UltDeduc { get; set; }

        public string? TipoAplicacion { get; set; }
        public bool? AplAjustaPlazo { get; set; }
        public bool? AplRetroactivo { get; set; }
        public bool? AplIntereses { get; set; }
        public bool? AplCargos { get; set; }
        public bool? AplPolizas { get; set; }

        public string? Usuario { get; set; }
        public string? Nota { get; set; }
    }
}
