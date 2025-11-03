namespace PgxAPI.Models.INV
{

    public class BodegaReporteInvDTO
    {
        public string cod_bodega { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class UnidadesReporteInvDTO
    {
        public string cod_unidad { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class DepartamentoReporteInvDTO
    {
        public string cod_departamento { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class ProveedoresInvDTO
    {
        public string cod_proveedor { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class LineasInvDTO
    {
        public string cod_prodclas { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

}
