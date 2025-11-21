namespace Galileo.Models.CPR
{
    public class TiposSuspensionDtoList
    {
        public int Total { get; set; }
        public List<TiposSuspensionDto> Suspensiones { get; set; } = new List<TiposSuspensionDto>();
    }

    public class TiposSuspensionDto
    {
        public string Cod_Suspension { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Activa { get; set; }
        public string Registro_Usuario { get; set; } = string.Empty;
        public DateTime Registro_Fecha { get; set; }
    }
}