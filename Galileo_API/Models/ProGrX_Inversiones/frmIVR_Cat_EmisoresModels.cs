namespace Galileo.Models.ProGrX_Inversiones
{
    public sealed class IvrCatEmisorDto
    {
        public string Cod_Emisor { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Cod_Sector { get; set; } = string.Empty;
        public string Sector_ItmX { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public string? Registro_Usuario { get; set; }
        public DateTime? Registro_Fecha { get; set; }
    }

    public sealed class IvrCatEmisorSectorDto
    {
        public string Cod_Sector { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }

    public sealed class IvrCatEmisorGuardarRequest
    {
        public string Cod_Emisor { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Cod_Sector { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public string Usuario { get; set; } = string.Empty;
    }
}
