namespace Galileo.Models.ProGrX.Fondos
{
    public class FndPlanesCopiaRequestDto
    {
        public string planbase { get; set; } = string.Empty;
        public string plandestino { get; set; } = string.Empty;
        public string descripciondestino { get; set; } = string.Empty;
        public short copiarMultas { get; set; }
        public short copiarPuntos { get; set; }
        public short copiarGeneral { get; set; }
        public short copiarCuentas { get; set; }
        public short copiarDestinos { get; set; }
        public short copiarEstadosPersona { get; set; }
        public short copiarPlazos { get; set; }
    }
}