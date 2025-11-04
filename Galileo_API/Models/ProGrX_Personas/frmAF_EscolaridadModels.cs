namespace PgxAPI.Models.ProGrX_Personas
{
    public class NivelEscolaridadData
    {
        public string Escolaridad_Tipo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Activo { get; set; } = false;
        public DateTime Registro_Fecha { get; set; }
        public string Registro_Usuario { get; set; } = string.Empty;
    }

    public class NivelEscolaridadLista
    {
        public int Total { get; set; }
        public List<NivelEscolaridadData> Lista { get; set; } = new List<NivelEscolaridadData>();
    }
}