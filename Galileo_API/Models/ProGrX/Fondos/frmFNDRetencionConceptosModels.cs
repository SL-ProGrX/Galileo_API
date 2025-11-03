namespace PgxAPI.Models.ProGrX.Fondos
{
    public class FNDRetencionConceptoData
    {
        public string RetencionCodigo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public string CodCuenta { get; set; } = string.Empty;
        public string CuentaMask { get; set; } = string.Empty;
        public string CtaDesc { get; set; } = string.Empty;
        public bool isNew { get; set; }
    }

    public class FNDRetencionConceptoLista
    {
        public int Total { get; set; }
        public List<FNDRetencionConceptoData> Lista { get; set; } = new List<FNDRetencionConceptoData>();
    }
}
