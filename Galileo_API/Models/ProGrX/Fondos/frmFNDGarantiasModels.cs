namespace Galileo.Models.ProGrX.Fondos
{
    public class FndGarantiaModel
    {
        public string Garantia_FND { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public required bool Activa { get; set; }
        public required bool IsNew { get; set; } // Para saber si es insert o update
        public string? Usuario { get; set; }
    }

    public class FndGarantiasLista
    {
        public int total { get; set; }
        public List<FndGarantiaModel> lista { get; set; } = new();
    }

    public class FndGarantiaValidaRequest
    {
        public string Garantia_FND { get; set; } = string.Empty;
    }

    public class FndGarantiaValidaResult
    {
        public int Existe { get; set; }
    }

    public class FndGarantiaAhorrosConsultaRequest
    {
        public string Garantia_FND { get; set; } = string.Empty;
        public string Cod_Estado { get; set; } = string.Empty;
    }

    public class FndGarantiaAhorrosConsultaResult
    {
        public int Linea_Id { get; set; }
        public int Cod_Operadora { get; set; }
        public string Cod_Plan { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public int Membresia_Inicio { get; set; }
        public int Membresia_Corte { get; set; }
        public decimal Porcentaje { get; set; }
        public bool Patrimonio { get; set; }
    }

    public class FndGarantiaAhorrosRegistroRequest
    {
        public string Garantia_FND { get; set; } = string.Empty;
        public string Cod_Estado { get; set; } = string.Empty;
        public required int Linea_Id { get; set; }
        public required int Membresia_Inicio { get; set; }
        public required int Membresia_Corte { get; set; }
        public required bool Patrimonio { get; set; }
        public required int Cod_Operadora { get; set; }
        public string Cod_Plan { get; set; } = string.Empty;
        public required decimal Porcentaje { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public string Accion { get; set; } = "A"; // "A"=Agregar, "E"=Editar
    }
}
