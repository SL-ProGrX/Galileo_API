namespace Galileo.Models.ProGrX.Fondos
{
    public class FndRetencionConceptoData
    {
        public string RetencionCodigo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public string CodCuenta { get; set; } = string.Empty;
        public string CuentaMask { get; set; } = string.Empty;
        public string CtaDesc { get; set; } = string.Empty;
        public bool isNew { get; set; }
    }

    public class FndRetencionConceptoLista
    {
        public int Total { get; set; }
        public List<FndRetencionConceptoData> Lista { get; set; } = new List<FndRetencionConceptoData>();
    }
}