namespace PgxAPI.Models.INV
{
    public class MarcasDTO
    {
        public string Cod_Marca { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public string Estado { get; set; } = string.Empty;
    }

    public class MarcasDataLista
    {
        public int? Total { get; set; }
        public List<MarcasDTO> Marcas { get; set; } = new List<MarcasDTO>();
    }

}
