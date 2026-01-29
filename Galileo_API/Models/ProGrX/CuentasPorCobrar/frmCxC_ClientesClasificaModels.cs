namespace Galileo_API.Models.ProGrX.CuentasPorCobrar
{

    public class CxCClientesClasificaLista
    {
        public int total { get; set; }
        public List<CxCClientesClasificaData> lista { get; set; } = new List<CxCClientesClasificaData>();
    }
    public class CxCClientesClasificaData
    {
        public string? Cod_categoria { get; set; } = string.Empty;
        public string? Descripcion { get; set; } = string.Empty;  
        public bool? Activo { get; set; }
        public bool? IsNew { get; set; }
    }
}
