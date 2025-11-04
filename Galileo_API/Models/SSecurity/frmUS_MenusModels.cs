namespace PgxAPI.Models.Security
{
    public class ResultadoCrearYEditarUsMenuDto
    {
        public int MENU_NODO { get; set; } // PK, not null
        public int NODO_PADRE { get; set; } // FK, null
        public string NODO_DESCRIPCION { get; set; } = string.Empty; // null
        public string? TIPO { get; set; } // null
        public string ICONO { get; set; } = string.Empty; // null
        public string? MODO { get; set; } // null
        public int MODAL { get; set; } // null
        public int ACCESOS_DLL_ID { get; set; } // null
        public string ACCESOS_DLL_CLS { get; set; } = string.Empty; // null
        public int PRIORIDAD { get; set; } // null
        public string FORMULARIO { get; set; } = string.Empty; // null
        public int MODULO { get; set; } // null
        public int? MIGRADO_WEB { get; set; } // null
        public string? ICONO_WEB { get; set; } // null

    }

    public class UsModuloDto
    {
        public int MODULO { get; set; }
        public string NOMBRE { get; set; } = string.Empty;
        public string DESCRIPCION { get; set; } = string.Empty;
        public int ACTIVO { get; set; }
        public string KEYENT { get; set; } = string.Empty;
        public List<UsFormularioDto> HijoFormularios { get; set; } = new List<UsFormularioDto>();
    }

    public class UsFormularioDto
    {
        // FORMULARIO es una clave primaria y no puede ser nula
        public string? FORMULARIO { get; set; }
        // MODULO es una clave primaria y foránea, y no puede ser nula
        public int MODULO { get; set; }
        // DESCRIPCION puede ser nula
        public string DESCRIPCION { get; set; } = string.Empty;
        // REGISTRO_FECHA puede ser nula
        public DateTime? REGISTRO_FECHA { get; set; }
        // REGISTRO_USUARIO puede ser nula
        public string REGISTRO_USUARIO { get; set; } = string.Empty;
        public int Existe { get; set; }

        public List<UsDerechosNewDto> Opciones { get; set; } = new List<UsDerechosNewDto>();
    }
}
