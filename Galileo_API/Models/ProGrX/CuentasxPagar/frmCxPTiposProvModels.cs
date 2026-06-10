namespace Galileo.Models.CxP
{
    public class TiposProveedorDto
    {
        public required int CodEmpresa { get; set; }
        public string CodClasificacion { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string NitCodigo { get; set; } = string.Empty;
        public required bool Activo { get; set; }
    }

    public class Proveedor
    {
        public string Cod_Proveedor { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }
}