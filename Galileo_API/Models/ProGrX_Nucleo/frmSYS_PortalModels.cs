namespace PgxAPI.Models.ProGrX_Nucleo
{
    public class frmSYS_PortalModels
    {
        // ============================
        // MENSAJES (GRID / LISTADO)
        // ============================
        public class Sys_MensajesPortal_Lista
        {
            public int total { get; set; }
            public List<Sys_MensajesPortal_ListaItem> lista { get; set; } = new();
        }
        public class Sys_MensajesPortal_ListaItem
        {
            public string codigo { get; set; } = string.Empty;
            public string titulo { get; set; } = string.Empty;

            // Catálogo SMTP
            public string smtp_id { get; set; } = string.Empty;

            // Tipo de formato (se muestra la desc, pero incluimos el código para el combo del FE)
            public string tipo_formato_cod { get; set; } = string.Empty;   // 3 chars
            public string tipo_formato_desc { get; set; } = string.Empty;

            public bool activa { get; set; }
        }

        // ============================
        // MENSAJES (DETALLE / EDITAR)
        // ============================
        public class Sys_MensajesPortal_DetalleModel
        {
            // --- Identificación y cabecera ---
            public string codigo { get; set; } = string.Empty;
            public string titulo { get; set; } = string.Empty;
            public string smtp_id { get; set; } = string.Empty;

            public string tipo_formato_cod { get; set; } = string.Empty;   // 3 chars
            public string tipo_formato_desc { get; set; } = string.Empty;  // opcional para lectura
            public bool activa { get; set; }

            // --- Contenido ---
            public string pie_01 { get; set; } = string.Empty;
            public string pie_02 { get; set; } = string.Empty;

            public string imagen_ruta { get; set; } = string.Empty;
            public int imagen_ancho { get; set; } = 600;   // default histórico VB6
            public int imagen_alto { get; set; } = 300;    // default histórico VB6

            // --- Ejecución ---
            public string procedimiento { get; set; } = string.Empty;

            // --- Activación ---
            // 'M' Manual, 'F' Fecha, 'D' Día del mes, 'C' Cada N días, 'E' Evento
            public char activacion { get; set; } = 'M';
            public string activacion_desc { get; set; } = string.Empty; // útil en lectura

            public DateTime? fecha_especifica { get; set; }    // F
            public int? dia_del_mes { get; set; }              // D (1..31, 32 = último día)
            public int? frecuencia_n_dias { get; set; }        // C (1..32)
            public DateTime? frecuencia_inicio { get; set; }   // C
            public string? evento_codigo { get; set; }         // E

            // --- Auditoría (solo lectura) ---
            public string registro_usuario { get; set; } = string.Empty;
            public DateTime? registro_fecha { get; set; }
            public string modifica_usuario { get; set; } = string.Empty;
            public DateTime? modifica_fecha { get; set; }
        }
        public const int DIA_ULTIMO = 32;
        // ============================
        // CATÁLOGOS
        // ============================
        public class Sys_MensajesPortal_SmtpDto
        {
            public string codigo { get; set; } = string.Empty; // p.ej. "CNF01"
            public string descripcion { get; set; } = string.Empty; // opcional
        }

        public class Sys_MensajesPortal_FormatoDto
        {
            public string codigo { get; set; } = string.Empty;       // 3 chars
            public string descripcion { get; set; } = string.Empty;  // "AutoGestión", "Cobros", ...
        }

        public class Sys_MensajesPortal_ActivacionDto
        {
            public char codigo { get; set; }         // 'M','F','D','C','E'
            public string descripcion { get; set; } = string.Empty; // "Manual", "Fecha", ...
        }

        public class Sys_MensajesPortal_EventoDto
        {
            public string codigo { get; set; } = string.Empty;       // "BEN","EST","CRD",...
            public string descripcion { get; set; } = string.Empty;  // Texto visible en combo
        }

        // Payload compuesto para la carga inicial del tab "Mensajes"
        public class Sys_MensajesPortal_InitDto
        {
            public List<Sys_MensajesPortal_ListaItem> lista { get; set; } = new();
            public List<Sys_MensajesPortal_SmtpDto> catalogo_smtps { get; set; } = new();
            public List<Sys_MensajesPortal_FormatoDto> catalogo_formatos { get; set; } = new();
            public List<Sys_MensajesPortal_ActivacionDto> catalogo_activaciones { get; set; } = new();
            public List<Sys_MensajesPortal_EventoDto> catalogo_eventos { get; set; } = new();
        }

        // ============================
        // PORTAL (PREFERENCIAS)
        // ============================
        public class Sys_MensajesPortal_PreferenciasModel
        {
            public string logo_url { get; set; } = string.Empty;
            public int logo_alto { get; set; } = 0;
            public int logo_ancho { get; set; } = 0;

            // Hex sin '#', p.ej. "4F46E5"
            public string color_set_hex { get; set; } = string.Empty;
        }
    }
}
