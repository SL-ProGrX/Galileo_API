namespace PgxAPI.Models.INV
{
    public class BodegaReporteInvDto
    {
        public string cod_bodega { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class UnidadesReporteInvDto
    {
        public string cod_unidad { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class DepartamentoReporteInvDto
    {
        public string cod_departamento { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class ProveedoresInvDto
    {
        public string cod_proveedor { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class LineasInvDto
    {
        public string cod_prodclas { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }
}