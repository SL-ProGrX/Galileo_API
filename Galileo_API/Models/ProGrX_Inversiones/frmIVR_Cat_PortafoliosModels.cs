namespace Galileo.Models.ProGrX_Inversiones
{
    public sealed class IvrCatPortafolioDto
    {
        public string Cod_Portafolio { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Desc_Corta { get; set; } = string.Empty;
        public bool? Activo { get; set; }
    }

    public sealed class IvrCatPortafolioAdministradorDto
    {
        public string Cod_Administrador { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Asignado { get; set; }
    }

    public sealed class IvrCatPortafolioGuardarRequest
    {
        public string Cod_Portafolio { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Desc_Corta { get; set; } = string.Empty;
        public bool? Activo { get; set; }
        public string Usuario { get; set; } = string.Empty;
    }
}
