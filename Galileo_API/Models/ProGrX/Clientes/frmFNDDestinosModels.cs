namespace Galileo.Models.ProGrX.Clientes
{
    public class FndDestinosData
    {
        public string? Cod_Destino { get; set; }
        public string? Descripcion { get; set; }
        public bool Activo { get; set; }
        public bool IsNew { get; set; } // Para distinguir entre insert y update
        public required string Usuario { get; set; } // Usuario que realiza la acción
    }

    public class FndDestinosLista
    {
        public int total { get; set; }
        public List<FndDestinosData> lista { get; set; } = new();
    }

    public class FndPlanesDestinoData
    {
        public string? Cod_Operadora { get; set; }
        public string? Cod_Plan { get; set; }
        public string? Descripcion { get; set; }
        public string? Cod_Destino { get; set; }
    }

    public class FndPlanesDestinoLista
    {
        public int total { get; set; }
        public List<FndPlanesDestinoData> lista { get; set; } = new();
    }

    public class FndAsignarPlanRequest
    {
        public string? Cod_Plan { get; set; }
        public string? Cod_Operadora { get; set; }
        public string? Cod_Destino { get; set; }
        public required string Usuario { get; set; }
        public bool Asignar { get; set; } // true = asignar, false = desasignar
    }
}