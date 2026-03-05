namespace Galileo_API.Models.ProGrX.Cobros
{
    public class CoReadecuacionCambioOperacionConsultaDto
    {
        public string cedula { get; set; } = string.Empty;
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public decimal int_cor_atrasado { get; set; }
        public decimal int_cor_venc { get; set; }
        public decimal int_moratorio { get; set; }
        public decimal saldo { get; set; }
        public decimal cargos { get; set; }
        public decimal polizas { get; set; }
        public decimal interes_total { get; set; }
        public decimal total_deuda { get; set; }
        public int plazo { get; set; }
        public decimal tasa { get; set; }
        public string tasa_label { get; set; } = "Tasa %";
        public int liq_tasa_x { get; set; }
        public decimal? tbp_puntos_add { get; set; }
        public string oficina_desc { get; set; } = string.Empty;
        public string oficina { get; set; } = string.Empty;
        public string recurso_desc { get; set; } = string.Empty;
        public decimal no_monto { get; set; }
        public int no_plazo { get; set; }
        public decimal no_tasa { get; set; }
        public decimal no_cuota { get; set; }
        public string fecha_server { get; set; } = string.Empty;
    }

    public class CoReadecuacionCambioOperacionObtenerResponse
    {
        public int id_tramite { get; set; }
        public CoReadecuacionCambioOperacionConsultaDto datos { get; set; } = new();
    }
    public class CoReadecuacionCambioOperacionAplicarRequest
    {
        public int id_tramite { get; set; }
        public string? notas { get; set; }
        public string? usuario_sesion { get; set; }

        public decimal no_monto { get; set; }
        public int no_plazo { get; set; }
        public decimal no_tasa { get; set; }
        public decimal no_cuota { get; set; }

        public bool chk_dia_pago { get; set; }
        public int sys_doc_version { get; set; }
        public int g_enlace { get; set; }
        public string? gstr_mascara { get; set; }
    }


    public class CoReadecuacionCambioOperacionAplicarResponse
    {
        public int operacion_original { get; set; }
        public int operacion_nueva { get; set; }

        public string tipo_documento { get; set; } = string.Empty;
        public string num_documento { get; set; } = string.Empty;

        public string mensaje { get; set; } = string.Empty; 
    }
    public class CoReadecuacionReporteOperacionNuevaRequest
    {
        public long id_solicitud { get; set; }
    }

    public class CoReadecuacionReporteOperacionNuevaDto
    {
        public long operacion_nueva { get; set; }
    }
}