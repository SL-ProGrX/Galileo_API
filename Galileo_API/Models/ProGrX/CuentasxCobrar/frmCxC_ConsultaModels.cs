namespace Galileo_API.Models.ProGrX.CuentasxCobrar
{

    public class CxCConsultaLista
    {
        public int total { get; set; }
        public List<CxCClientesClasificaData> lista { get; set; } = new List<CxCClientesClasificaData>();
    }
    public class CxCConsultaData
    {
        public string? Cod_categoria { get; set; } = string.Empty;
        public string? Descripcion { get; set; } = string.Empty;
        public bool? Activo { get; set; }
        public bool? IsNew { get; set; }
    }
}
