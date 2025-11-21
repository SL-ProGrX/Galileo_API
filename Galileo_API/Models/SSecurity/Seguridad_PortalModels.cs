namespace Galileo.Models
{
    public class UsAdminClientesDto
    {
        public string USUARIO { get; set; } = string.Empty;
        public int COD_EMPRESA { get; set; }
        public DateTime? REGISTRO_FECHA { get; set; }
        public string REGISTRO_USUARIO { get; set; } = string.Empty;
        public int R_LOCAL_GRANTS { get; set; }
        public int R_LOCAL_USERS { get; set; }
        public int R_LOCAL_KEY_RESET { get; set; }
        public int R_GLOBAL_DIR_SEARCH { get; set; }
        public int R_ADMIN_REVIEW { get; set; }
        public DateTime SALIDA_FECHA { get; set; }
        public string SALIDA_USUARIO { get; set; } = string.Empty;
        public int ACTIVO { get; set; }

    }

    public class AdminAccessDto
    {
        public bool Admin_Portal { get; set; }
        public bool Rol_AdminView { get; set; }
        public bool Rol_DirGlobal { get; set; }
        public bool Rol_LocalUsers { get; set; }
        public bool Rol_Permisos { get; set; }
        public bool Rol_ResetKeys { get; set; }
        public string ResultMsg { get; set; } = string.Empty;
    }

    public class UsuarioBloqueoDto
    {
        public int Bloqueo { get; set; }
        public DateTime BloqueoT { get; set; }
        public bool BloqueoI { get; set; }
        public string Usuario { get; set; } = string.Empty;
    }

    public class UsuarioCondicionDto
    {
        public int KEY_RENEW_SESION { get; set; }
        public DateTime KEY_BLOQUEO { get; set; }
        public int KEY_BLOQUEOI { get; set; }
        public int KEY_CADUCIDAD { get; set; }
        public int KEY_ADMIN { get; set; }
        public string Usuario { get; set; } = string.Empty;
    }

    public class UsuarioVencimientoDto
    {
        public int Vencida { get; set; }
        public int Renovacion { get; set; }
        public int Dias { get; set; }
        public string Usuario { get; set; } = string.Empty;
    }

    public class PgxClienteDto
    {
        public int COD_EMPRESA { get; set; } // Primary Key, not null
        public string COD_VENDEDOR { get; set; } = string.Empty; // FK, not null
        public string NOMBRE_LARGO { get; set; } = string.Empty; // null
        public string NOMBRE_CORTO { get; set; } = string.Empty; // null
        public byte[]? LOGO_CLIENTE { get; set; } // image, null
        public char? ESTADO { get; set; }// null
        public string IDENTIFICACION { get; set; } = string.Empty; // null
        public string EMAIL_01 { get; set; } = string.Empty;// null
        public string EMAIL_02 { get; set; } = string.Empty; // null
        public string TEL_CELL { get; set; } = string.Empty; // null
        public string TEL_TRABAJO { get; set; } = string.Empty; // null
        public string TEL_AUXILIAR { get; set; } = string.Empty; // null
        public string WEB_SITE { get; set; } = string.Empty;// null
        public string FACEBOOK { get; set; } = string.Empty; // null
        public DateTime? SUSCRIPCION_INICIAL { get; set; } // null
        public DateTime? SUSCRIPCION_VENCE { get; set; } // null
        public decimal? SUSCRIPCION_MENSUALIDAD { get; set; } // null
        public decimal? SUSCRIPCION_ANUAL { get; set; } // null
        public string PGX_CORE_SERVER { get; set; } = string.Empty; // null
        public string PGX_CORE_DB { get; set; } = string.Empty; // null
        public string PGX_CORE_USER { get; set; } = string.Empty;// null
        public string PGX_CORE_KEY { get; set; } = string.Empty;// null
        public string PGX_ANALISIS_SERVER { get; set; } = string.Empty;// null
        public string PGX_ANALISIS_DB { get; set; } = string.Empty;// null
        public string PGX_ANALISIS_USER { get; set; } = string.Empty; // null
        public string PGX_ANALISIS_KEY { get; set; } = string.Empty;// null
        public string PGX_AUXILIAR_SERVER { get; set; } = string.Empty;// null
        public string PGX_AUXILIAR_DB { get; set; } = string.Empty;// null
        public string PGX_AUXILIAR_USER { get; set; } = string.Empty;// null
        public string PGX_AUXILIAR_KEY { get; set; } = string.Empty;// null
        public string PGX_PRUEBAS_SERVER { get; set; } = string.Empty; // null
        public string PGX_PRUEBAS_DB { get; set; } = string.Empty; // null
        public string PGX_PRUEBAS_USER { get; set; } = string.Empty; // null
        public string PGX_PRUEBAS_KEY { get; set; } = string.Empty; // null
        public string REGISTRO_USUARIO { get; set; } = string.Empty; // null
        public DateTime? REGISTRO_FECHA { get; set; } // null
        public string DIRECCION { get; set; } = string.Empty; // null
        public string APTO_POSTAL { get; set; } = string.Empty; // null
        public string PAIS { get; set; } = string.Empty; // null
        public short? PROVINCIA { get; set; } // null
        public int? CANTON { get; set; } // null
        public string DISTRITO { get; set; } = string.Empty; // null
        public string COD_PAIS { get; set; } = string.Empty; // null
        public string COD_PAIS_N1 { get; set; } = string.Empty; // null
        public string COD_PAIS_N2 { get; set; } = string.Empty; // null
        public string COD_PAIS_N3 { get; set; } = string.Empty; // null
        public string COD_CLASIFICACION { get; set; } = string.Empty; // null
        public string TIPO_ID { get; set; } = string.Empty; // null
        public int? PGX_PRUEBAS_ACTIVO { get; set; } // null
        public string URL_App { get; set; } = string.Empty; // null
        public string URL_Web { get; set; } = string.Empty; // null
        public string URL_Logo { get; set; } = string.Empty; // null
        public int? URL_App_Activo { get; set; } // null
        public int? URL_Web_Activo { get; set; } // null
        public int? URL_Logo_Activo { get; set; } // null
    }

    public class SbgSegInicializaResultDto
    {
        public string BloqueoMsg { get; set; } = string.Empty;
        public string CondicionMsg { get; set; } = string.Empty;
        public string Vencimiento_VencidaMsg { get; set; } = string.Empty;
        public string Vencimiento_RenovacionMsg { get; set; } = string.Empty;
        public string AppStatusMsg { get; set; } = string.Empty;
    }

    public class UsMenuDto
    {
        public int MENU_NODO { get; set; } // PK, not null
        public int NODO_PADRE { get; set; } // FK, null
        public string NODO_DESCRIPCION { get; set; } = string.Empty; // null
        public string TIPO { get; set; } = string.Empty; // null
        public string ICONO { get; set; } = string.Empty; // null
        public string MODO { get; set; } = string.Empty; // null
        public int MODAL { get; set; } // null
        public int ACCESOS_DLL_ID { get; set; } // null
        public string ACCESOS_DLL_CLS { get; set; } = string.Empty; // null
        public int PRIORIDAD { get; set; } // null
        public string FORMULARIO { get; set; } = string.Empty; // null
        public int MODULO { get; set; } // null
        public int Acceso { get; set; } // null
        public int MIGRADO_WEB { get; set; }
        public string ICONO_WEB { get; set; } = string.Empty;
    }

    public class UsIconWeb
    {
        public string label { get; set; } = string.Empty;
        public string value { get; set; } = string.Empty;
        public string iconMenu { get; set; } = string.Empty;
    }
}