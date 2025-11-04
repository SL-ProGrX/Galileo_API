namespace PgxAPI.Models.ProGrX_Personas
{
    public class EstadoLaboralData
    {
        public string Estado_Laboral { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Activo { get; set; } = false;
        public DateTime Registro_Fecha { get; set; }
        public string Registro_Usuario { get; set; } = string.Empty;
    }

    public class EstadoLaboralLista
    {
        public int Total { get; set; }
        public List<EstadoLaboralData> Lista { get; set; } = new List<EstadoLaboralData>();
    }
}