namespace PgxAPI.Models.INV
{
    public class UnidadMedicion
    {
        public string Cod_Unidad { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }

    public class UnidadMedicionDto
    {
        public string Cod_Unidad { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Hacienda { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string Registro_Usuario { get; set; } = string.Empty;
        public DateTime? Registro_Fecha { get; set; }
    }

    public class UnidadesDataLista
    {
        public int Total { get; set; }
        public List<UnidadMedicionDto> Unidades { get; set; } = new List<UnidadMedicionDto>();
    }
}