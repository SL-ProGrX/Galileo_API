namespace Galileo_API.Models.ProGrX.Cobros
{
    public class CoControlUsuariosRolAsignarAntiguedadRequest
    {
        public string? usuario { get; set; }
        public string? cod_antiguedad { get; set; }
        public bool? asignar { get; set; }
        public string? usuario_sesion { get; set; }
    }

    public class CoControlUsuariosRolAsignarGarantiaRequest
    {
        public string? usuario { get; set; }
        public string? garantia { get; set; }
        public bool? asignar { get; set; }
        public string? usuario_sesion { get; set; }
    }

    public class CoControlUsuariosRolAsignarOficinaRequest
    {
        public string? usuario { get; set; }
        public string? cod_oficina { get; set; }
        public bool? asignar { get; set; }
        public string? usuario_sesion { get; set; }
    }

    public class CoControlUsuariosRolAsignarInstitucionRequest
    {
        public string? usuario { get; set; }
        public string? cod_institucion { get; set; }
        public bool? asignar { get; set; }
        public string? usuario_sesion { get; set; }
    }

    public class CoControlUsuariosRolCopiaRequest
    {
        public string? us_origen { get; set; }
        public string? us_destino { get; set; }
        public string? usuario_sesion { get; set; }
    }

    public class CoControlUsuariosRolLimpiaRequest
    {
        public string? usuario_sesion { get; set; }
    }

    public class CoControlUsuariosRolAntiguedadItem
    {
        public string cod_antiguedad { get; set; } = "";
        public string descripcion { get; set; } = string.Empty;
        public int asignado { get; set; }
    }

    public class CoControlUsuariosRolGarantiaItem
    {
        public string garantia { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public int asignado { get; set; }
    }

    public class CoControlUsuariosRolOficinaItem
    {
        public string cod_oficina { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public int asignado { get; set; }
    }

    public class CoControlUsuariosRolInstitucionItem
    {
        public string cod_institucion { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public int asignado { get; set; }
    }
}
