namespace Galileo.Models.AF
{
    public class BeneficioGrupoDataLista
    {
        public int total { get; set; }
        public List<BeneficioGrupoData> beneficios { get; set; } = new List<BeneficioGrupoData>();
    }

    public class BeneficioGrupoData
    {
        [System.Text.Json.Serialization.JsonRequired]
        public string cod_grupo { get; set; } = string.Empty;
        public string? descripcion { get; set; }

        /// <summary>Indica que la fila es nueva en la tabla de Angular; no persiste en base de datos.</summary>
        [System.Text.Json.Serialization.JsonRequired]
        public bool isNew { get; set; }
    }

    public class BeneficioUsuariosDataLista
    {
        public int total { get; set; }
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