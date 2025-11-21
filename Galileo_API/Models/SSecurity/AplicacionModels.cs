namespace Galileo.Models.Security
{
    public class Aplicacion
    {
        public string Cod_App { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Activa { get; set; }
        public string Estado { get; set; } = string.Empty;
        public DateTime Registro_Fecha { get; set; }
        public string Registro_Usuario { get; set; } = string.Empty;
    }

    public class Bloqueo
    {
        public int Cod_Linea { get; set; }
        public string Cod_App { get; set; } = string.Empty;
        public DateTime Fecha_Bloqueo { get; set; }
        public string Version_Bloqueada { get; set; } = string.Empty;
        public DateTime Registro_Fecha { get; set; }
        public string Registro_Usuario { get; set; } = string.Empty;
    }

    public class Actualizacion
    {
        public string Cod_App { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public DateTime Fecha_Libera { get; set; }
        public string Notas_Descarga { get; set; } = string.Empty;
        public DateTime Registro_Fecha { get; set; }
        public string Registro_Usuario { get; set; } = string.Empty;
    }
}
