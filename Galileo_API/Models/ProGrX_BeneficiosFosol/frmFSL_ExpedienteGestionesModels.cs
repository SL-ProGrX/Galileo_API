namespace PgxAPI.Models.FSL
{
    public class fslGestionesListaDatos
    {
        public string item { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class fslGestionAgregar
    {
        public long cod_expediente { get; set; }
        public string cod_gestion { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
    }
}
