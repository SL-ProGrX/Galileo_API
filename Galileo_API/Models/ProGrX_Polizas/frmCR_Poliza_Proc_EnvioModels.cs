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
        public DateTime? fecha_corte { get; set; } 
    }

    public class CrdPolizaConsultaResponseDto
    {
        public string tipo { get; set; } = string.Empty;
        public List<GridColumnDto> columns { get; set; } = new();
        public List<Dictionary<string, object?>> rows { get; set; } = new();
        public int total { get; set; }
    }

    public static class GridConstants
    {
        // =========================
        // Fields (Data binding)
        // =========================
        public static class Fields
        {
            public const string Cedula = "CEDULA";
            public const string Genero = "Genero";
            public const string FechaNacimiento = "FECHA_NACIMIENTO";
            public const string Email = "EMAIL";
            public const string Telefono = "TELEFONO";
            public const string Movimiento = "MOVIMIENTO";
            public const string NombreCompleto = "NOMBRE_COMPLETO";
            public const string Nacionalidad = "Nacionalidad";

            public const string CreditoMonto = "CREDITO_MONTO";
            public const string CreditoSaldo = "CREDITO_SALDO";
        }

        // =========================
        // Titles (UI text)
        // =========================
        public static class Titles
        {
            public const string Cedula = "Cédula";
            public const string Genero = "Genero";
            public const string FechaNacimiento = "Fecha Nac.";
            public const string Email = "Correo Electrónico";
            public const string Telefono = "Teléfono";
            public const string Movimiento = "Movimiento";
            public const string NombreCompleto = "Nombre Completo";
            public const string Nacionalidad = "Nacionalidad";

            public const string MontoAsegurado = "Monto Asegurado";
            public const string FechaEmision = "Fecha Emisión";

            // Crédito (CRD)
            public const string CrdOperacion = "CRD.Operación";
            public const string CrdCodigo = "CRD.Código";
            public const string CrdMonto = "CRD.Monto";
            public const string CrdSaldo = "CRD.Saldo";
            public const string CrdEstado = "CRD.Estado";
            public const string CrdOps = "CRD.Ops";
        }

        // =========================
        // Alignment
        // =========================
        public static class Align
        {
            public const string Left = "left";
            public const string Center = "center";
            public const string Right = "right";
        }

        // =========================
        // Formats
        // =========================
        public static class Formats
        {
            public const string DateYMD = "date:yyyy-MM-dd";
            public const string Numeric2 = "n2";
        }
    }


  
}

   