namespace Galileo.Models.AF
{
    public class BeneficioPersona
    {
        public string Apellido1 { get; set; } = string.Empty;
        public string Apellido2 { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string EstadoCivil { get; set; } = string.Empty;
        public string Genero { get; set; } = string.Empty;
        public DateTime? FechaNacimiento { get; set; }
        public DateTime? FechaIngreso { get; set; }
        public string LugarTrabajo { get; set; } = string.Empty;
        public string NivelAcademico { get; set; } = string.Empty;
        public string Ocupacion { get; set; } = string.Empty;
        public string PaisNacimiento { get; set; } = string.Empty;
        public string Nacionalidad { get; set; } = string.Empty;
        public string Email1 { get; set; } = string.Empty;
        public string Email2 { get; set; } = string.Empty;
        public string AptoPostal { get; set; } = string.Empty;
        public string Provincia { get; set; } = string.Empty;
        public string Canton { get; set; } = string.Empty;
        public string Distrito { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public List<string> ListaTelefonos { get; set; } = new List<string>();

        public string estadolaboral { get; set; } = string.Empty;

    }
}