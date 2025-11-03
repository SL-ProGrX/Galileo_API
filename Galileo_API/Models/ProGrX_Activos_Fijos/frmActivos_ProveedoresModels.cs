namespace PgxAPI.Models.ProGrX_Activos_Fijos
{
    public class ActivosProveedoresLista
    {
        public int total { get; set; }
        public List<ActivosProveedoresData> lista { get; set; } = new List<ActivosProveedoresData>();
    }

    public class ActivosProveedoresData
    {
        public string cod_proveedor { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool activo { get; set; } = true;
        public string usuario { get; set; } = string.Empty;
        public bool isNew { get; set; } = false;
    }
}
