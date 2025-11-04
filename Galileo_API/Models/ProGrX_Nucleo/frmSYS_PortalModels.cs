namespace PgxAPI.Models.ProGrX_Nucleo
{
    public class FrmSysPortalModels
    {
        // ============================
        // MENSAJES (GRID / LISTADO)
        // ============================
        public class SysMensajesPortalLista
        {
            public int total { get; set; }
            public List<SysMensajesPortalListaItem> lista { get; set; } = new();
        }
        
        public class SysMensajesPortalListaItem
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
        public class SysMensajesPortalDetalleModel
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
        public class SysMensajesPortalSmtpDto
        {
            public string codigo { get; set; } = string.Empty; // p.ej. "CNF01"
            public string descripcion { get; set; } = string.Empty; // opcional
        }

        public class SysMensajesPortalFormatoDto
        {
            public string codigo { get; set; } = string.Empty;       // 3 chars
            public string descripcion { get; set; } = string.Empty;  // "AutoGestión", "Cobros", ...
        }

        public class SysMensajesPortalActivacionDto
        {
            public char codigo { get; set; }         // 'M','F','D','C','E'
            public string descripcion { get; set; } = string.Empty; // "Manual", "Fecha", ...
        }

        public class SysMensajesPortalEventoDto
        {
            public string codigo { get; set; } = string.Empty;       // "BEN","EST","CRD",...
            public string descripcion { get; set; } = string.Empty;  // Texto visible en combo
        }

        // Payload compuesto para la carga inicial del tab "Mensajes"
        public class SysMensajesPortalInitDto
        {
            public List<SysMensajesPortalListaItem> lista { get; set; } = new();
            public List<SysMensajesPortalSmtpDto> catalogo_smtps { get; set; } = new();
            public List<SysMensajesPortalFormatoDto> catalogo_formatos { get; set; } = new();
            public List<SysMensajesPortalActivacionDto> catalogo_activaciones { get; set; } = new();
            public List<SysMensajesPortalEventoDto> catalogo_eventos { get; set; } = new();
        }

        // ============================
        // PORTAL (PREFERENCIAS)
        // ============================
        public class SysMensajesPortalPreferenciasModel
        {
            public string logo_url { get; set; } = string.Empty;
            public int logo_alto { get; set; } = 0;
            public int logo_ancho { get; set; } = 0;
            // Hex sin '#', p.ej. "4F46E5"
            public string color_set_hex { get; set; } = string.Empty;
        }
    }
}