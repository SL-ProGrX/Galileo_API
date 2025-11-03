namespace PgxAPI.Models.INV
{
    public class DepartamentosDTO
    {
        public string Cod_Departamento { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public string? Estado { get; set; } = string.Empty;
    }

    public class AsignacionesDTO
    {
        public string Cod_Prodclas { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Cod_Alter { get; set; } = string.Empty;
        public string Costeo { get; set; } = string.Empty;
        public string Cod_Cuenta { get; set; } = string.Empty;
        public string Valuacion { get; set; } = string.Empty;
        public string Cod_Departamento { get; set; } = string.Empty;
    }

    public class DepartamentosDataLista
    {
        public int Total { get; set; }
        public List<DepartamentosDTO> Departamentos { get; set; } = new List<DepartamentosDTO>();
    }

}
