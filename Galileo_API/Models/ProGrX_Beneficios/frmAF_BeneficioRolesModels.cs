namespace PgxAPI.Models.AF
{
    public class BeneficioGrupoDataLista
    {
        public int Total { get; set; }
        public List<BeneficioGrupoData> beneficios { get; set; } = new List<BeneficioGrupoData>();
    }

    public class BeneficioGrupoData
    {
        public string cod_grupo { get; set; } = string.Empty;
        public string? descripcion { get; set; }
    }

    public class BeneficioUsuariosDataLista
    {
        public int Total { get; set; }
        public List<BeneficioUsuariosData> usuarios { get; set; } = new List<BeneficioUsuariosData>();
    }

    public class BeneficioUsuariosData
    {
        public string nombre { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public bool activo { get; set; } = false;
    }
}