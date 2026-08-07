using Galileo.Models;

namespace Galileo.Models.GEN
{
    public class CcEstadoCuentaInicialData
    {
        public DateTime fecha_servidor { get; set; }

        public CcEstadoCuentaConfiguracionData configuracion
        {
            get;
            set;
        } = new();

        public List<DropDownListaGenericaModel> periodos_cierre
        {
            get;
            set;
        } = new();

        public List<DropDownListaGenericaModel> periodos_excedentes
        {
            get;
            set;
        } = new();

        public List<DropDownListaGenericaModel> instituciones
        {
            get;
            set;
        } = new();

        public List<DropDownListaGenericaModel> estados_persona
        {
            get;
            set;
        } = new();
    }

    public class CcEstadoCuentaConfiguracionData
    {
        public string nombre_empresa { get; set; } =
            string.Empty;

        public int sys_ccss_ind { get; set; } = 0;

        public int ec_visible_patrimonio { get; set; } = 0;

        public int ec_visible_fondos { get; set; } = 0;

        public int ec_visible_creditos { get; set; } = 0;

        public int ec_visible_fianzas { get; set; } = 0;

        public string estado_cuenta { get; set; } =
            string.Empty;

        public string constancia_crd_encabezado { get; set; } =
            string.Empty;
    }

    public class CcEstadoCuentaPersonaRequest
    {
        public string cedula { get; set; } =
            string.Empty;

        public string usuario { get; set; } =
            string.Empty;
    }

    public class CcEstadoCuentaPersonaData
    {
        public string cedula { get; set; } =
            string.Empty;

        public string nombre { get; set; } =
            string.Empty;

        public string email { get; set; } =
            string.Empty;
    }

    public class CcEstadoCuentaDepartamentoRequest
    {
        public int cod_institucion { get; set; } = 0;
    }

    public class CcEstadoCuentaSeccionRequest
    {
        public int cod_institucion { get; set; } = 0;

        public string cod_departamento { get; set; } =
            string.Empty;
    }

    public class CcEstadoCuentaEmailRequest
    {
        public string usuario { get; set; } =
            string.Empty;

        public string cedula { get; set; } =
            string.Empty;

        public string email { get; set; } =
            string.Empty;

        public DateTime? fecha_corte { get; set; }
    }

    public class CcEstadoCuentaEmailMasivoRequest
    {
        public string usuario { get; set; } =
            string.Empty;

        public int cod_institucion { get; set; } = 0;

        public string cod_departamento { get; set; } =
            string.Empty;

        public string cod_seccion { get; set; } =
            string.Empty;

        public string cod_estado { get; set; } =
            string.Empty;

        public DateTime? fecha_corte { get; set; }
    }

    internal class CcEstadoCuentaPeriodoData
    {
        public object? idx { get; set; }

        public string itmx { get; set; } =
            string.Empty;
    }

    internal class CcEstadoCuentaAccesoData
    {
        public int persona_id { get; set; } = 0;

        public int autorizacion_id { get; set; } = 0;
    }

    public class CcEstadoCuentaReporteBitacoraRequest
    {
        public string usuario { get; set; } =
            string.Empty;

        public string tipo_reporte { get; set; } =
            string.Empty;

        public string cedula { get; set; } =
            string.Empty;

        public int id_periodo { get; set; } = 0;

        public bool por_segmentos { get; set; } = false;
    }
}