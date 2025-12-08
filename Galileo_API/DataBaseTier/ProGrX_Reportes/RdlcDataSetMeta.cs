namespace Galileo.DataBaseTier.ProGrX_Reportes
{
    public sealed class RdlcDataSetMeta
    {
        public string DataSetName { get; set; } = string.Empty;
        public string? CommandText { get; set; }
        public string? CommandType { get; set; } // "StoredProcedure" | "Text" | null
        public List<(string Name, string? ValueExpr)> QueryParams { get; } = new();
    }

    internal sealed class VbFunctionSig
    {
        public string Name { get; set; } = string.Empty;
        public string ReturnType { get; set; } = "Decimal";
        public List<string> Parameters { get; set; } = new();
    }
}
