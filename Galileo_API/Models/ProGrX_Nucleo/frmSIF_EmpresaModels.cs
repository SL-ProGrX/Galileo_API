namespace PgxAPI.Models.ProGrX_Nucleo
{
    public class FrmSifEmpresaModel
    {
        public int id_empresa { get; set; }

        // --- Datos b�sicos de empresa ---
        public string nombre { get; set; } = string.Empty;
        public string cedula_juridica { get; set; } = string.Empty;
        public string apto_postal { get; set; } = string.Empty;
        public string telefonoemp { get; set; } = string.Empty;
        public string fax { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string sitio_web { get; set; } = string.Empty;

        // Contabilidad / cuentas
        public int? cod_empresa_enlace { get; set; }  
        public string? cod_cuenta_no_cfg { get; set; }
        public string? cuenta_desc { get; set; } // Descripci�n (vista vCNTX_CUENTAS_LOCAL)

        // --- Pagar� ---
        public string pag_nomlargo { get; set; } = string.Empty;
        public string pag_nomcorto { get; set; } = string.Empty;
        public string pag_cedjurle { get; set; } = string.Empty;
        public string pag_domicilio { get; set; } = string.Empty;

        public string representante_legal { get; set; } = string.Empty;
        public string representante_id { get; set; } = string.Empty;
        public string representante_calidades { get; set; } = string.Empty;

        public string pag_seccion_01 { get; set; } = string.Empty;
        public string pag_seccion_02 { get; set; } = string.Empty;

        // --- Estado de cuenta ---
        public bool usar_estado_comercial { get; set; }
        public string ec_nota01 { get; set; } = string.Empty;
        public string ec_nota02 { get; set; } = string.Empty;

        public bool ec_visible_patrimonio { get; set; }
        public bool ec_visible_fondos { get; set; }
        public bool ec_visible_creditos { get; set; }
        public bool ec_visible_fianzas { get; set; }
        public bool ec_visible_excedentes { get; set; }
        public bool ec_visible_disponible { get; set; }

        public string liq_boleta_pie { get; set; } = string.Empty;

        // --- Misi�n / Visi�n / Slogan ---
        public string mision { get; set; } = string.Empty;
        public string vision { get; set; } = string.Empty;
        public string slogan { get; set; } = string.Empty;

        // --- Consentimiento ---
        public string consentimiento_contacto_titulo { get; set; } = string.Empty;
        public string consentimiento_contacto_texto { get; set; } = string.Empty;

        // --- Constancias ---
        public string constancia_crd_encabezado { get; set; } = string.Empty;
        public string constancia_crd_pie { get; set; } = string.Empty;
        public string constancia_pat_encabezado { get; set; } = string.Empty;
        public string constancia_pat_pie { get; set; } = string.Empty;
        public bool constancia_fecha_vinculacion { get; set; }

        // --- Logos / im�genes ---
        public byte[]? logo { get; set; }
        public byte[]? fondo_pantalla { get; set; }

        // --- Bloqueo de fechas ---
        public DateTime? fecha_congela { get; set; }

        // --- Otros ---
        public bool? sinpe_activo { get; set; }
    }

    // Imagen (logo o fondo)
    public class EmpresaImagenDto
    {
        public string nombre_archivo { get; set; } = string.Empty;
        public byte[] contenido { get; set; } = Array.Empty<byte>();
    }

    // Bloqueo de fechas (SP)
    public class BloqueoFechaRequest
    {
        public DateTime fecha { get; set; }       // Solo fecha, la hora 22:00 se agrega en DB
        public char accion { get; set; }          // 'B' (bloquear) o 'D' (desbloquear)
        public string usuario { get; set; } = string.Empty;
    }

    // Combo para contabilidades
    public class ComboContabilidadDto
    {
        public int idx { get; set; }
        public string itmx { get; set; } = string.Empty; // NOMBRE
        public string descripcion { get; set; } = string.Empty;
    }

    // Lookup de cuentas
    public class CuentaLookupDto
    {
        public string cod_cuenta_mask { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }
}