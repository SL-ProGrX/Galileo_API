namespace Galileo.Models.ProGrX.Cobros
{
    public class FrmCOCobroFiadoresPendienteData
    {
        public long id_solicitud { get; set; }
        public string codigo { get; set; } = "";
        public string cedula { get; set; } = "";
        public string nombre { get; set; } = "";
        public int n_cuota { get; set; }
        public decimal mora_financiera { get; set; }
        public decimal saldo { get; set; }

        public string notifica_fecha { get; set; } = "";
        public string estadoPersona_desc { get; set; } = "";
        public string linea_desc { get; set; } = "";
        public string institucion_desc { get; set; } = "";

        public decimal disponible_ahorros { get; set; }
        public decimal disponible_excedentes { get; set; }
        public decimal disponible_fondos { get; set; }
    }

    public class FrmCOCobroFiadoresPendientesListaResult
    {
        public int total { get; set; } = 0;
        public List<FrmCOCobroFiadoresPendienteData> lista { get; set; } = new();
    }

    public class FrmCOCobroFiadoresPendientesConsultaDto
    {
        public int institucionId { get; set; } = 0;

        public int estadoPersonaId { get; set; } = 0;

        public int cuotasAtrasadas { get; set; } = 2;

        public bool mostrarDisponibles { get; set; } = false;
    }
    public class FrmCOCobroFiadoresActivoData
    {
        public long id_solicitud { get; set; }
        public string codigo { get; set; } = "";
        public string cedula { get; set; } = "";
        public string nombre { get; set; } = "";
        public decimal cuota { get; set; }

        public string d_operacion { get; set; } = "";
        public string d_codigo { get; set; } = "";
        public string d_cedula { get; set; } = "";
        public string d_nombre { get; set; } = "";

        public string estadoPersona_desc { get; set; } = "";
        public string linea_desc { get; set; } = "";
    }

    public class FrmCOCobroFiadoresActivosListaResult
    {
        public int total { get; set; } = 0;
        public List<FrmCOCobroFiadoresActivoData> lista { get; set; } = new();
    }

    public class FrmCOCobroFiadoresActivosConsultaDto
    {
        public int institucionId { get; set; } = 0;
        public int estadoPersonaId { get; set; } = 0;
    }
    public class FrmCOCobroFiadoresConsultaData
    {
        public long id_solicitud { get; set; }
        public string codigo { get; set; } = "";
        public string cedula { get; set; } = "";
        public string nombre { get; set; } = "";
        public int n_cuota { get; set; }

        public decimal mora_financiera { get; set; }
        public decimal saldo_original { get; set; }

        public string acccion_tipo { get; set; } = "";
        public string accion_fecha { get; set; } = "";

        public decimal recaudo { get; set; }
        public decimal saldo_actual { get; set; }

        public string notifica_fecha { get; set; } = "";
        public string estadoPersona_desc { get; set; } = "";
        public string linea_desc { get; set; } = "";
        public string institucion_desc { get; set; } = "";

        public short cbrExterno { get; set; }
    }

    public class FrmCOCobroFiadoresConsultasListaResult
    {
        public int total { get; set; } = 0;
        public List<FrmCOCobroFiadoresConsultaData> lista { get; set; } = new();
    }

    public class FrmCOCobroFiadoresConsultasConsultaDto
    {
        public string inicio { get; set; } = "";
        public string corte { get; set; } = "";
        public string accion { get; set; } = "A";
    }

    public class FrmCOCobroFiadoresAccionBulkDto
    {
        public List<long> ids { get; set; } = new();
    }
}