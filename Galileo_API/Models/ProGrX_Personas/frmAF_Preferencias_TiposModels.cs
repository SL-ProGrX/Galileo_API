namespace Galileo.Models.ProGrX_Personas
{
    public class PreferenciaTipoData
    {
        public string Cod_Preferencia { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Activa { get; set; } = false;
        public DateTime Registro_Fecha { get; set; }
        public string Registro_Usuario { get; set; } = string.Empty;
    }

    public class PreferenciaTipoLista
    {
        public int Total { get; set; }
        public List<PreferenciaTipoData> Lista { get; set; } = new List<PreferenciaTipoData>();
    }
}