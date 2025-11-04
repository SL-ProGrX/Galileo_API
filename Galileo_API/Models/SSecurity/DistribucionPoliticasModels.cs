namespace PgxAPI.Models.Security
{
    public class PaisObtenerDto
    {
        public string COD_PAIS { get; set; } = string.Empty;
        public string DESCRIPCION { get; set; } = string.Empty;
        public int ZONA_HORARIA { get; set; }
        public bool ACTIVO { get; set; }
        public string N1_NOMBRE { get; set; } = string.Empty;
        public string N2_NOMBRE { get; set; } = string.Empty;
        public string N3_NOMBRE { get; set; } = string.Empty;
        public DateTime REGISTRO_FECHA { get; set; }
        public string REGISTRO_USUARIO { get; set; } = string.Empty;
    }

    public class ProvinciasObtenerDto
    {
        public string COD_PAIS_N1 { get; set; } = string.Empty;
        public string DESCRIPCION { get; set; } = string.Empty;
        public bool ACTIVO { get; set; }
    }

    public class CantonesObtenerDto
    {
        public string COD_PAIS_N2 { get; set; } = string.Empty;
        public string DESCRIPCION { get; set; } = string.Empty;
        public bool ACTIVO { get; set; }
    }

    public class DistritosObtenerDto
    {
        public string COD_PAIS_N2 { get; set; } = string.Empty;
        public string DESCRIPCION { get; set; } = string.Empty;
        public bool ACTIVO { get; set; }
    }

    public class GuardarDto
    {
        public string VModifica { get; set; } = string.Empty;
        public string Provincia { get; set; } = string.Empty;
        public string Canton { get; set; } = string.Empty;
        public string Distrito { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string TagProvincia { get; set; } = string.Empty;
        public string TagCanton { get; set; } = string.Empty;
    }
}
