namespace Galileo_API.Models.ProGrX_Polizas
{
    public class CrdPolizaGridMetaRequestDto
    {
        public string? cod_poliza { get; set; } = string.Empty;

        /// <summary>
        /// "C" = Crédito, "R" = Retención (equivalente a Mid(cboAnalisis.Text,1,1))
        /// </summary>
        public string analisis { get; set; } = "C";
    }

    public class GridColumnDto
    {
        public string field { get; set; } = string.Empty;   // clave del dato (ej: "Cedula")
        public string title { get; set; } = string.Empty;   // texto visible (ej: "Cédula")
        public int width { get; set; }                      // ancho sugerido
        public string align { get; set; } = "left";         // left|center|right
        public string? format { get; set; }                 // "date:yyyy-MM-dd", "n2", etc.
    }

    public class CrdPolizaGridMetaResponseDto
    {
        public string tipo { get; set; } = string.Empty;               // PPC, PREN, PVEH, ...
        public List<GridColumnDto> columns { get; set; } = new();
    }

    public class CrdPolizaConsultaRequestDto
    {
        public string cod_poliza { get; set; } = string.Empty;
        public string analisis { get; set; } = "C"; // "C" o "R"
        public DateTime fecha_corte { get; set; }
    }

    public class CrdPolizaConsultaResponseDto
    {
        public string tipo { get; set; } = string.Empty;
        public List<GridColumnDto> columns { get; set; } = new();
        public List<Dictionary<string, object?>> rows { get; set; } = new();
        public int total { get; set; }
    }
}
