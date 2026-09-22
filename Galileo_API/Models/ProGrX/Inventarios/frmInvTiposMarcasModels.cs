namespace Galileo.Models.INV
{
    public class MarcasDto
    {
        public string cod_Marca { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public required bool activo { get; set; }
        public string estado { get; set; } = string.Empty;
    }

    public class MarcasDataLista
    {
        public int? total { get; set; }
        public List<MarcasDto> marcas { get; set; } = new List<MarcasDto>();
    }
}