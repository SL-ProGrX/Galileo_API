namespace Galileo_API.Models.ProGrX_Procesos
{
    public class FrmCcFndSolidarioModels
    {

        public sealed class FondoSolidarioContext
        {
            public int? CodEmpresa { get; init; }
            public int? CodInstitucion { get; init; } 
            public string Usuario { get; init; } = string.Empty;

            // TODO(GLOBALES)
            public decimal? GlngFechaCR { get; init; }
            public decimal? FechaProcesoSiguiente { get; init; }
            public decimal? FechaProcesoAnterior { get; init; }

            public DateTime? FechaServidor { get; init; }
        }

    }
}
