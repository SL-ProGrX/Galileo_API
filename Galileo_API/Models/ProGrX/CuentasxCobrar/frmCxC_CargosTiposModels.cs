namespace Galileo_API.Models.ProGrX.CuentasxCobrar
{

    public class CxCCargosTiposLista
    {
        public int total { get; set; }
        public List<CxCCargosTiposData> lista { get; set; } = new List<CxCCargosTiposData>();
    }
    public class CxCCargosTiposData
    {
        public string? Cod_cargo { get; set; } = string.Empty;
        public string? Descripcion { get; set; } = string.Empty;
        public string? Tipo { get; set; } = string.Empty;
        public string? Cod_cuenta { get; set; } = string.Empty;
        public string? cod_cuenta_mask { get; set; } = string.Empty;
        public bool? Activo { get; set; }
        public bool? IsNew { get; set; }
    }
}
