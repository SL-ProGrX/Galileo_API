namespace Galileo_API.Models.ProGrX.CuentasxCobrar
{
    public class CxCParametrosLista
    {
        public int total { get; set; }
        public List<CxCParametrosData> lista { get; set; } = new List<CxCParametrosData>();
    }

    public class CxCParametrosData
    {
        public string? Cod_Parametro { get; set; } = string.Empty;
        public string? Descripcion { get; set; } = string.Empty;
        public string? Notas { get; set; } = string.Empty;
        public string? Valor { get; set; } = string.Empty;
        public string? Tipo { get; set; } = string.Empty;
        public DateTime Inicio_Fecha { get; set; }
        public string? Visible { get; set; } = string.Empty;
        public string? Modifica_Usuario { get; set; } = string.Empty;
        public DateTime Modifica_Fecha { get; set; }
        public string? cuentaMasck { get; set; } = string.Empty;
        public string? cuentaDetalle { get; set; } = string.Empty;

    }
}
