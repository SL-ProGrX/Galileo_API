namespace PgxAPI.Models.ProGrX_Personas
{
    public class ZonasData
    {
        public string Cod_Zona { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Activa { get; set; } = false;
        public string Registro_Usuario { get; set; } = string.Empty;
        public DateTime? Registro_Fecha { get; set; }
        public bool isNew { get; set; }
    }

    public class ZonasLista
    {
        public int Total { get; set; }
        public List<ZonasData> Lista { get; set; } = new List<ZonasData>();
    }

    public class ZonaUsuarioAsignadoData
    {
        public string Codigo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Asignado { get; set; } = false;
    }

    public class ZonaInstitucionAsignadaData
    {
        public string Codigo { get; set; } = string.Empty;
        public string Desc_Corta { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Asignado { get; set; } = false;
    }
}
