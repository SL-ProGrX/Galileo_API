namespace Galileo.Models.ProGrX.Fondos
{
    public class FndGestionesBuscarContratosParams
    {
        public int Top { get; set; } = 100;
        public string Usuario { get; set; } = string.Empty;
        public int? Cod_Operadora { get; set; }
        public string? Cod_Plan { get; set; }
        public int? Cod_Contrato { get; set; }
        public string? Cedula { get; set; }
        public string? Nombre { get; set; }
        public required int CodEmpresa { get; set; }
    }

    public class FndGestionesBuscarContratosResult
    {
        public string Operadora { get; set; } = string.Empty;
        public int Cod_Operadora { get; set; }
        public string Cod_Plan { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public int Cod_Contrato { get; set; }
        public string Cedula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
    }

    public class FndGestionesContratoParams
    {
        public required int CodEmpresa { get; set; }
        public required int CodContrato { get; set; }
        public required int CodOperadora { get; set; }
        public string CodPlan { get; set; } = string.Empty;
    }

    public class FndGestionesContratoResult
    {
        public string Cedula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public int Plazo { get; set; }
        public decimal Monto { get; set; }
        public decimal Aportes { get; set; }
        public decimal Rendimiento { get; set; }
        public string Inc_Tipo { get; set; } = string.Empty;
        public decimal Inc_Anual { get; set; }
        public string Operacion { get; set; } = string.Empty;
    }

    public class FndGestionesContratosRenovacionParams
    {
        public required int CodEmpresa { get; set; }
        public required int CodOperadora { get; set; }
        public string Gestion { get; set; } = "O"; // "O", "P", "C"
        public string? CodPlan { get; set; }
        public int? CodContrato { get; set; }
    }

    public class FndGestionesContratosRenovacionResult
    {
        public string Cod_Plan { get; set; } = string.Empty;
        public int Cod_Contrato { get; set; }
        public string Inc_Tipo { get; set; } = string.Empty;
        public decimal Inc_Anual { get; set; }
        public decimal Monto { get; set; }
        public DateTime Fecha_Inicio { get; set; }
        public DateTime? Ult_Renovacion { get; set; }
        public int Plazo { get; set; }
    }

    public class FndGestionesContratoActualizarItem
    {
        public string CodPlan { get; set; } = string.Empty;
        public int CodContrato { get; set; }
        public decimal Monto { get; set; }
    }

    public class FndGestionesContratoActualizarParams
    {
        public required int CodEmpresa { get; set; }
        public required int CodOperadora { get; set; }
        public List<FndGestionesContratoActualizarItem> Contratos { get; set; } = new();
    }

    public class FndGestionesContratoActualizarResult
    {
        public bool Success { get; set; }
        public int Updated { get; set; }
        public List<int> NoActualizados { get; set; } = new();
    }
}
