namespace Galileo.Models.ProGrX.Cajas
{
    public class CajasUsuariosData
    {
        public string cod_caja { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public string nombre_usuario { get; set; } = string.Empty;
        public string contrasena { get; set; } = string.Empty;
        public DateTime? contrasena_renovacion { get; set; }
        public required bool bloqueo { get; set; }
        public DateTime? registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public required bool isNew { get; set; }
    }

    public class CajasUsuariosListadoUsuarioData
    {
        public string usuario { get; set; } = string.Empty;
        public string nombre_usuario { get; set; } = string.Empty;
        public bool tiene_cajas { get; set; }
    }

    public class CajasUsuariosHistData
    {
        public int linea { get; set; }
        public string cod_caja { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime? salida_fecha { get; set; }
        public string salida_usuario { get; set; } = string.Empty;
    }

    public class CajasUsuariosCajaListaData
    {
        public string cod_caja { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool asignado { get; set; }
    }

}