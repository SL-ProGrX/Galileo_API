namespace PgxAPI.Models.ProGrX_Personas
{
    public class NoCotizaRangosData
    {
        public int Linea_Id { get; set; }
        public int Dia_Desde { get; set; }
        public int Dia_Hasta { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public bool Activo { get; set; } = false;
        public DateTime Registro_Fecha { get; set; }
        public string Registro_Usuario { get; set; } = string.Empty;
        public DateTime? Modifica_Fecha { get; set; }
        public string? Modifica_Usuario { get; set; }
    }

    public class NoCotizaRangosLista
    {
        public int Total { get; set; }
        public List<NoCotizaRangosData> Lista { get; set; } = new List<NoCotizaRangosData>();
    }
}