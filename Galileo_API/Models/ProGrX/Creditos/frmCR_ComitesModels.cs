namespace Galileo_API.Models.ProGrX.Creditos
{
    public class CrComitesLista
    {
        public int total { get; set; }
        public List<CrComitesData> lista { get; set; } = new();
    }

    public class CrComitesData
    {
        public int id_comite { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public int acta { get; set; }
        public string abreviatura { get; set; } = string.Empty;
        public string orden { get; set; } = string.Empty;
        public string tipo_aprobacion { get; set; } = string.Empty;
        public int naprobaciones { get; set; }
        public decimal rng_inicio { get; set; }
        public decimal rng_corte { get; set; }
        public bool linea_filtra { get; set; }
        public bool estado { get; set; }
        public bool isNew { get; set; }
    }

    public class CrComitesGuardarRequest
    {
        public int? id_comite { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public int? acta { get; set; }
        public string abreviatura { get; set; } = string.Empty;
        public string orden { get; set; } = string.Empty;
        public string tipo_aprobacion { get; set; } = string.Empty;
        public int? naprobaciones { get; set; }
        public decimal? rng_inicio { get; set; }
        public decimal? rng_corte { get; set; }
        public bool? linea_filtra { get; set; }
        public bool? estado { get; set; }
        public bool? isNew { get; set; }
    }

    public class CrComitesGuardarResult
    {
        public int id_comite { get; set; }
    }

    public class CrComitesEliminarResult
    {
        public int pass { get; set; }
        public string mensaje { get; set; } = string.Empty;
    }

    public class CrComitesGarantiasLista
    {
        public int total { get; set; }
        public List<CrComitesGarantiasData> lista { get; set; } = new();
    }

    public class CrComitesGarantiasData
    {
        public int id_comite { get; set; }
        public string cod_garantia { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public decimal rng_inicio { get; set; }
        public decimal rng_corte { get; set; }
        public bool isNew { get; set; }
    }

    public class CrComitesGarantiasGuardarRequest
    {
        public int? id_comite { get; set; }
        public string cod_garantia { get; set; } = string.Empty;
        public decimal? rng_inicio { get; set; }
        public decimal? rng_corte { get; set; }
    }

    public class CrComitesLineasLista
    {
        public int total { get; set; }
        public List<CrComitesLineasData> lista { get; set; } = new();
    }

    public class CrComitesLineasData
    {
        public int id_comite { get; set; }
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool asignado { get; set; }
    }

    public class CrComitesLineasAsignarRequest
    {
        public int? id_comite { get; set; }
        public string codigo { get; set; } = string.Empty;
        public bool? asignado { get; set; }
    }

    public class CrComitesNivelAprobacionDto
    {
        public string item { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }
}