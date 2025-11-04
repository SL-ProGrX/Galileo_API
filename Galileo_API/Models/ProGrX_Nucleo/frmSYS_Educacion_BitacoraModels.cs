namespace PgxAPI.Models.ProGrX_Nucleo
{
    public class SysEducacionListData
    {
        public string? IdX { get; set; }
        public string? Tipo { get; set; }
        public string? ItmX { get; set; }
    }

    public class SysPadronData
    {
        public string? Identificacion { get; set; }
        public string? Nombre { get; set; }
    }

    public class SysPadronLista
    {
        public int total { get; set; }
        public List<SysPadronData>? lista { get; set; }
    }

    public class SysEducacionLogData
    {
        public string? Cedula { get; set; }
        public string? Nombre { get; set; }
        public DateTime? Registro_Fecha { get; set; }
        public string? Registro_Usuario { get; set; }
        public string? Universidad { get; set; }
        public string? Nivel { get; set; }
        public string? Carrera { get; set; }
        public string? Especialidad { get; set; }
        public string? Ciclo { get; set; }
        public string? Ciclo_Anio { get; set; }
        public string? Beneficiario_Id { get; set; }
        public string? Beneficiario { get; set; }
        public string? Parentesco { get; set; }
    }

    public class SysEducacionLogDataFiltros : SysEducacionLogData
    {
        public DateTime? Registro_Fecha_Inicio { get; set; }
        public DateTime? Registro_Fecha_Corte { get; set; }
        public string? Ciclo_Anio_Inicio { get; set; }
        public string? Ciclo_Anio_Corte { get; set; }

    }
}