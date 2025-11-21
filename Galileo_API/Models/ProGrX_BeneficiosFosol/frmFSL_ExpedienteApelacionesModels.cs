namespace Galileo.Models.FSL
{
    public class FslTipoApelacion
    {
        public string item { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class FslApleacionAplicar
    {
        public long cod_expediente { get; set; }
        public string cod_apelacion { get; set; } = string.Empty;
        public string presentaCedula { get; set; } = string.Empty;
        public string presentaNombre { get; set; } = string.Empty;
        public string presentaNotas { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
    }

    public class FslResolucionApleacion
    {
        public string cod_comite { get; set; } = string.Empty;
        public List<FslResolucionDatos> miembros { get; set; } = new List<FslResolucionDatos>();
        public string cod_resolucion { get; set; } = string.Empty;
        public string resolucion_notas { get; set; } = string.Empty;
        public string resolucion_usuario { get; set; } = string.Empty;
        public string resolucion_estado { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public long cod_expediente { get; set; }
        public List<FslApelacionDatos> apelaciones { get; set; } = new List<FslApelacionDatos>();
    }
}
