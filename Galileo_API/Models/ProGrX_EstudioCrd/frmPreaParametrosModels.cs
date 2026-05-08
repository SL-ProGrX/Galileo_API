namespace Galileo_API.Models.ProGrX_EstudioCrd
{
    public class FrmPreaParametrosModels
    {
        public class PreaParametroModel
        {
            public string CodParametro { get; set; } = string.Empty;
            public string Descripcion { get; set; } = string.Empty;
            public string Valor { get; set; } = string.Empty;
            public DateTime? FechaActualiza { get; set; }
            public string UsuarioActualiza { get; set; } = string.Empty;
            public string ValorAnterior { get; set; } = string.Empty;
        }

        public class PreaParametroHistoricoModel
        {
            public int IdHistorico { get; set; }
            public string CodParametro { get; set; } = string.Empty;
            public string Valor { get; set; } = string.Empty;
            public DateTime? FechaActualiza { get; set; }
            public string UsuarioActualiza { get; set; } = string.Empty;
        }

        public class PreaParametroActualizarRequest
        {
            public string CodParametro { get; set; } = string.Empty;
            public string Valor { get; set; } = string.Empty;
            public string Usuario { get; set; } = string.Empty;
        }
    }
}
