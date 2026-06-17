using Galileo.Models;

namespace Galileo_API.Models.ProGrX_Pasivos
{
    public class FrmCrApaOperacionAcreedorDto
    {
        public string cod_acreedor { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
    }

    public class FrmCrApaOperacionContactoDto
    {
        public string cod_contacto { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string tel_cel { get; set; } = string.Empty;
        public string tel_trabajo { get; set; } = string.Empty;
        public string tel_fax { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
    }

    public class FrmCrApaOperacionGridDto
    {
        public string operacion { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public decimal cuota { get; set; }
        public decimal saldo { get; set; }
        public string estado { get; set; } = string.Empty;
    }

    public class FrmCrApaOperacionListaDto
    {
        public int total { get; set; }
        public List<FrmCrApaOperacionGridDto> lista { get; set; } = new();
    }

    public class FrmCrApaOperacionDatosDto
    {
        public string cod_acreedor { get; set; } = string.Empty;
        public string acreedor_desc { get; set; } = string.Empty;
        public string operacion { get; set; } = string.Empty;
        public decimal porc_responsabilidad { get; set; }
        public string tipo { get; set; } = string.Empty;
        public string tipo_desc { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public decimal saldo { get; set; }
        public decimal tasa { get; set; }
        public int plazo { get; set; }
        public decimal cuota { get; set; }
        public DateTime? fecha_formaliza { get; set; }
        public DateTime? fecha_primer_pago { get; set; }
        public int dia_de_pago { get; set; }
        public decimal comision_admin { get; set; }
        public string estado { get; set; } = string.Empty;
        public string estado_desc { get; set; } = string.Empty;
        public string responsabilidad_base { get; set; } = string.Empty;
        public string comision_base { get; set; } = string.Empty;
        public string cod_oficina { get; set; } = string.Empty;
        public string oficina_desc { get; set; } = string.Empty;
        public string periocidad_pago { get; set; } = string.Empty;
        public string periodicidad_desc { get; set; } = string.Empty;
        public string cod_divisa { get; set; } = string.Empty;
        public string divisa_desc { get; set; } = string.Empty;
        public decimal tipo_cambio { get; set; }
        public int? cod_linea { get; set; }
        public string linea_desc { get; set; } = string.Empty;
        public DateTime? fecha_actualiza { get; set; }
    }

    public class FrmCrApaOperacionGuardarRequest
    {
        public string cod_acreedor { get; set; } = string.Empty;
        public string operacion { get; set; } = string.Empty;
        public required decimal porc_responsabilidad { get; set; }
        public string tipo { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public required decimal monto { get; set; }
        public required decimal tasa { get; set; }
        public required int plazo { get; set; }
        public required decimal cuota { get; set; }
        public required DateTime fecha_formaliza { get; set; }
        public required DateTime fecha_primer_pago { get; set; }
        public required int dia_de_pago { get; set; }
        public required decimal comision_admin { get; set; }
        public string responsabilidad_base { get; set; } = string.Empty;
        public string comision_base { get; set; } = string.Empty;
        public string cod_oficina { get; set; } = string.Empty;
        public string periocidad_pago { get; set; } = string.Empty;
        public string cod_divisa { get; set; } = string.Empty;
        public required decimal tipo_cambio { get; set; }
        public int? cod_linea { get; set; }
        public string usuario { get; set; } = string.Empty;
        public required bool edita_todo { get; set; }
    }

    public class FrmCrApaOperacionPagoGridDto
    {
        public int npago { get; set; }
        public DateTime? pago_fecha { get; set; }
        public string documento { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public string estado { get; set; } = string.Empty;
    }

    public class FrmCrApaOperacionPagoListaDto
    {
        public int total { get; set; }
        public List<FrmCrApaOperacionPagoGridDto> lista { get; set; } = new();
    }

    public class FrmCrApaOperacionPagoDatosDto
    {
        public string cod_acreedor { get; set; } = string.Empty;
        public string operacion { get; set; } = string.Empty;
        public int npago { get; set; }
        public string documento { get; set; } = string.Empty;
        public string pago_usuario { get; set; } = string.Empty;
        public DateTime? pago_fecha { get; set; }
        public DateTime? fecha_pago { get; set; }
        public string estado { get; set; } = string.Empty;
        public string estado_desc { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public string tesoreria_solicitud { get; set; } = string.Empty;
        public string tesoreria_usuario { get; set; } = string.Empty;
        public DateTime? tesoreria_fecha { get; set; }
        public decimal detalle_intereses { get; set; }
        public decimal detalle_cargos { get; set; }
        public decimal detalle_amortiza { get; set; }
        public decimal detalle_tasa { get; set; }
        public decimal detalle_saldo { get; set; }
        public decimal detalle_comision { get; set; }
        public string forma_pago { get; set; } = string.Empty;
        public string forma_pago_desc { get; set; } = string.Empty;
        public string cedula_autorizado { get; set; } = string.Empty;
        public string nombre_autorizado { get; set; } = string.Empty;
    }

    public class FrmCrApaOperacionPagoGuardarRequest
    {
        public string cod_acreedor { get; set; } = string.Empty;
        public string operacion { get; set; } = string.Empty;
        public required DateTime fecha_pago { get; set; }
        public required decimal monto { get; set; }
        public required decimal detalle_intereses { get; set; }
        public required decimal detalle_cargos { get; set; }
        public required decimal detalle_amortiza { get; set; }
        public required decimal detalle_tasa { get; set; }
        public required decimal detalle_saldo { get; set; }
        public required decimal detalle_comision { get; set; }
        public string documento { get; set; } = string.Empty;
        public string forma_pago { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
    }

    public class FrmCrApaOperacionUltimoPagoDto
    {
        public decimal ultima_cuota { get; set; }
        public decimal ultima_tasa { get; set; }
        public decimal ultimo_saldo { get; set; }
    }

    public class FrmCrApaOperacionCatalogoDto
    {
        public string idx { get; set; } = string.Empty;
        public string itmx { get; set; } = string.Empty;
    }

    public class FrmCrApaOperacionAutorizadoDto
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
    }

    public class FrmCrApaOperacionAsignarAutorizadoRequest
    {
        public string cod_acreedor { get; set; } = string.Empty;
        public string operacion { get; set; } = string.Empty;
        public required int npago { get; set; }
        public string cedula_autorizado { get; set; } = string.Empty;
    }

    public class FrmCrApaOperacionesListaParameters
    {
        public int codEmpresa { get; set; }
        public FiltrosLazyLoadData? filtros { get; set; }
        public string? sqlCount { get; set; }
        public string? sqlData { get; set; }
        public object parametros { get; set; } = new();
    }
}
