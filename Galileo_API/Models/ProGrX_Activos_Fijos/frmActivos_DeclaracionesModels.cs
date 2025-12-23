namespace Galileo.Models.ProGrX_Activos_Fijos
{
    public class ActivosDeclaracion
    {
        public int id_declara { get; set; }
        public string tipo { get; set; } = "";
        public string tipo_desc { get; set; } = "";
        public string estado { get; set; } = "";
        public string estado_desc { get; set; } = "";
        public string notas { get; set; } = "";
        public string fecha_inicio { get; set; } = "";
        public string fecha_corte { get; set; } = "";
        public string? registro_fecha { get; set; }
        public string? registro_usuario { get; set; }
        public string? cerrado_fecha { get; set; }
        public string? cerrado_usuario { get; set; }
        public string? procesado_fecha { get; set; }
        public string? procesado_usuario { get; set; }
        public bool isNew { get; set; } = false;
    }

    public class ActivosDeclaracionResumen
    {
        public int id_declara { get; set; }
        public string tipo_desc { get; set; } = "";
        public string estado_desc { get; set; } = "";
        public string fecha_inicio { get; set; } = "";
        public string fecha_corte { get; set; } = "";
        public string notas { get; set; } = "";
        public string registro_fecha { get; set; } = "";
        public string registro_usuario { get; set; } = "";
        public string? cerrado_fecha { get; set; }
        public string? cerrado_usuario { get; set; }
        public string? procesado_fecha { get; set; }
        public string? procesado_usuario { get; set; }
    }

    public class ActivosDeclaracionLista
    {
        public int total { get; set; }
        public List<ActivosDeclaracionResumen> lista { get; set; } = new();
    }

    public class ActivosDeclaracionGuardarRequest
    {
        public int? id_declara { get; set; }
        public string notas { get; set; } = "";
        public string tipo { get; set; } = "";
        public string fecha_inicio { get; set; } = "";
        public string fecha_corte { get; set; } = "";
        public string usuario { get; set; } = "";
        public bool isNew { get; set; } = false;
    }
    
    public class ActivosDeclaracionResult
    {
        public int id_declara { get; set; }
    }

}
