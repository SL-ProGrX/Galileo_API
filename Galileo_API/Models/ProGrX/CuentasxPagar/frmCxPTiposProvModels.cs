namespace PgxAPI.Models.CxP
{
    public class TiposProveedorDto
    {
        public int CodEmpresa { get; set; }
        public string CodClasificacion { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string NitCodigo { get; set; } = string.Empty;
        public bool Activo { get; set; }

    }

    public class Proveedor
    {
        public string Cod_Proveedor { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }
}
