namespace Galileo_API.Models.ProGrX_Polizas
{
    public class PolizasEstadosReclamosDto
    {
        public required int Id_Estado { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public short Activo { get; set; }
    }

    public class PolizasEstadosReclamosExisteResult
    {
        public int Existe { get; set; }
    }

    public class PolizasEstadosReclamosSaveParams
    {
        public required  int Id_Estado { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public short? Activo { get; set; }
        public string Usuario { get; set; } = string.Empty;
    }

    public class PolizasEstadosReclamosDeleteParams
    {
        public required int Id_Estado { get; set; }
    }
}
