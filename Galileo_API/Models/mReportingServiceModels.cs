namespace PgxAPI.Models
{
    public class FrmReporteGlobal
    {
        public int codEmpresa { get; set; }
        public string? parametros { get; set; }
        public string? nombreReporte { get; set; }
        public string? usuario { get; set; }
        public string? cod_reporte { get; set; }
        public string? folder { get; set; }
        public string? codeSection { get; set; }
    }
    sealed class VbFunctionSig // Removed 'private' modifier
    {
        public string Name { get; set; } = string.Empty;
        public string ReturnType { get; set; } = "Decimal"; // default razonable
        public List<string> Parameters { get; set; } = new();
    }
}
