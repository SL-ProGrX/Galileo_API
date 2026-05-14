namespace Galileo.Models.ProGrX.Fondos
{
    public class FndConsultaDto
    {
        public string operadora { get; set; } = string.Empty;
        public int cod_operadora { get; set; }
        public string cod_plan { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public int cod_contrato { get; set; }
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
    }

    public class FndConsultaFiltros
    {
        public int lineas { get; set; }
        public int? cod_operadora { get; set; }
        public string? cod_plan { get; set; } 
        public int? cod_contrato { get; set; }
        public string? cedula { get; set; }
        public string? nombre { get; set; }
        public string usuario { get; set; } = string.Empty;
    }
}
