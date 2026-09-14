namespace Galileo_API.Models.ProGrX.Cajas
{

    public class CajasCambioClaveRequestDto
    {
        [System.ComponentModel.DataAnnotations.Required]
        public string Usuario { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.Required]
        public string ClaveActual { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.Required]
        public string ClaveNueva { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.Required]
        public string Cajas { get; set; } = string.Empty;
    }

    public class CajasUsuarioDto
    {
        public string? Codigo { get; set; }
        public string? Descripcion { get; set; }
        public int  Periodicidad_Contrasena { get; set; }
    }


}

