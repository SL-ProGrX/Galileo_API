namespace Galileo.Models.ProGrX.Fondos
{
    public class FndPlanesCopiaRequestDto
    {
        public string planbase { get; set; } = string.Empty;
        public string plandestino { get; set; } = string.Empty;
        public string descripciondestino { get; set; } = string.Empty;
        public required short copiarMultas { get; set; }
        public required short copiarPuntos { get; set; }
        public required short copiarGeneral { get; set; }
        public required short copiarCuentas { get; set; }
        public required short copiarDestinos { get; set; }
        public required short copiarEstadosPersona { get; set; }
        public required short copiarPlazos { get; set; }
    }
}