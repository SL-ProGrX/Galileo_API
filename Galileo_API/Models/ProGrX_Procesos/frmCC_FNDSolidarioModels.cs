using System.Data.Common;

namespace Galileo_API.Models.ProGrX_Procesos
{
    public class FrmCcFndSolidarioModels
    {

        public sealed class FondoSolidarioContext
        {
            public int? CodEmpresa { get; init; }
            public int? CodInstitucion { get; init; } 
            public string Usuario { get; init; } = string.Empty;
 
            public decimal? GlngFechaCR { get; init; }
            public decimal? FechaProcesoSiguiente { get; init; }
            public decimal? FechaProcesoAnterior { get; init; }

            public DateTime? FechaServidor { get; init; }
        }

        public class FndsPasoConfig
        {
            public IEnumerable<(string Cedula, decimal Monto)> Rows { get; set; } = [];
            public decimal MontoBase { get; set; }
            public string Garantia { get; set; } = "";
            public Action<DbConnection, DbTransaction, int, decimal>? Actualizar { get; set; }
        }

    }
}
