namespace PgxAPI.Models.INV
{
    public class MarcasDto
    {
        public string Cod_Marca { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public string Estado { get; set; } = string.Empty;
    }

    public class MarcasDataLista
    {
        public int? Total { get; set; }
        public List<MarcasDto> Marcas { get; set; } = new List<MarcasDto>();
    }
}