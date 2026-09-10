namespace Galileo.Models.Security
{
    public class PaisObtenerDto
    {
        public string CodPais { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public int ZonaHoraria { get; set; }
        public bool Activo { get; set; }
        public string N1Nombre { get; set; } = string.Empty;
        public string N2Nombre { get; set; } = string.Empty;
        public string N3Nombre { get; set; } = string.Empty;
        public DateTime RegistroFecha { get; set; }
        public string RegistroUsuario { get; set; } = string.Empty;
    }

    public class ProvinciasObtenerDto
    {
        public string CodPaisN1 { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }

    public class CantonesObtenerDto
    {
        public string CodPaisN2 { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }

    public class DistritosObtenerDto
    {
        public string CodPaisN3 { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Activo { get; set; }
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
