namespace Galileo.Models.INV
{
    public class DepartamentosDto
    {
        public string Cod_Departamento { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public string? Estado { get; set; } = string.Empty;
    }

    public class AsignacionesDto
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
        public List<DepartamentosDto> Departamentos { get; set; } = new List<DepartamentosDto>();
    }
}