namespace Galileo.Models.CxP
{
    public class CxpProveedoresDataLista
    {
        public int Total { get; set; }
        public List<CxpProveedorData>? Proveedores { get; set; }
    }

    public class CxpProveedorData
    {
        public string Cod_Proveedor { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }
}