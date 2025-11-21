namespace Galileo.Models.ProGrX_Personas
{
    public class CanalTipoData
    {
        public string Canal_Tipo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Activo { get; set; } = false;
        public DateTime Registro_Fecha { get; set; }
        public string Registro_Usuario { get; set; } = string.Empty;
    }

    public class CanalTipoLista
    {
        public int Total { get; set; }
        public List<CanalTipoData> Lista { get; set; } = new List<CanalTipoData>();
    }
}