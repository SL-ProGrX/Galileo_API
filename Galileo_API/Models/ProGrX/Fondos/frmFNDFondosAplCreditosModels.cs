namespace Galileo_API.Models.ProGrX.Fondos
{
    public class FndFondosAplCreditosPlanModel
    {
        public string? CodPlan { get; set; }
        public string? Descripcion { get; set; }
        public string OrderBy { get; set; } = "CodPlan";
    }

    public class FndFondosAplCreditosListaParams
    {
        public required int CodOperadora { get; set; }
        public string? CodPlan { get; set; }
        public string? Tipo { get; set; } // 'M' o 'E'
        public required int CodInstitucion { get; set; }
        public required int CodEmpresa { get; set; }
    }

    public class FndFondosAplCreditosListaResult
    {
        public string? Cedula { get; set; }
        public string? Nombre { get; set; }
        public decimal Disponible { get; set; }
        public decimal Morosidad { get; set; }
        public decimal Saldos { get; set; }
    }

    public class FndFondosAplCreditosAplicacionGeneralParams
    {
        public string Usuario { get; set; } = string.Empty;
        public short AplicaMora { get; set; } = 1;
        public short AplicaCtaTransito { get; set; } = 1;
        public short AplicaExtra { get; set; } = 0;
        public int Institucion { get; set; } = 0;
        public required int CodEmpresa { get; set; }
    }

    public class FndFondosAplCreditosAplicacionGeneralResult
    {
        public string? TipoDoc { get; set; }
        public string? NumDoc { get; set; }
    }

    public class FndFondosAplCreditosAplicacionParams
    {
        public required int CodOperadora { get; set; }
        public string CodPlan { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public short AplicaMora { get; set; } = 1;
        public short AplicaCtaTransito { get; set; } = 1;
        public short AplicaExtra { get; set; } = 0;
    }

    public class FndFondosAplCreditosAplicacionResult
    {
        public string? TipoDoc { get; set; }
        public string? NumDoc { get; set; }
    }

    public class FndFondosAplCreditosResumenResult
    {
        public int CodOperadora { get; set; }
        public string CodPlan { get; set; } = string.Empty;
        public string CodMoneda { get; set; } = string.Empty;
        public string PlanDesc { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public int Contratos { get; set; }
    }
}
