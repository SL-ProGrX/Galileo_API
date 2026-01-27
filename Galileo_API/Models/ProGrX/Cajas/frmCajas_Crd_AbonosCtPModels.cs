namespace Galileo_API.Models.ProGrX.Cajas
{
    public sealed class CajasCrdAbonosCtPData
    {
        public long id_solicitud { get; set; }

        public decimal saldo { get; set; }

        public decimal Saldo_mes { get; set; }

        public string proceso { get; set; } = string.Empty;

        public string divisa { get; set; } = "COL";

        public decimal? interesv { get; set; }

        // Use @ to escape the reserved keyword 'int'
        public int @int { get; set; }

        public int plazo { get; set; }

        public decimal interesc { get; set; }

        public decimal amortiza { get; set; }

        public decimal? fecult { get; set; }  

        public long prideduc { get; set; }

        public int? iva_aplica { get; set; }

        public int? opex { get; set; }

        public decimal cuota { get; set; }

        public string codigo { get; set; } = string.Empty;

        public string cedula { get; set; } = string.Empty;

        public int? cuotas_planilla { get; set; }

        public int? cuotas_directas { get; set; }

        public int? meses { get; set; }

        public DateTime? fechaforp { get; set; }

        public string base_Calculo { get; set; } = string.Empty;

        // socios
        public string nombre { get; set; } = string.Empty;

        // catalogo
        public string descripcion { get; set; } = string.Empty;

        public string retencion { get; set; } = "N"; // 'S' / 'N'

        public string poliza { get; set; } = "N";    // 'S' / 'N'

        public decimal? porc_cargo_cancelacion { get; set; }

        public int? anticipo_meses { get; set; }

        public int? diasActivo { get; set; }

        public int? autPagoAnt { get; set; } 

        public string lineaDesc { get; set; } = string.Empty;

        public string oficinaDesc { get; set; } = string.Empty;

        public string recursoDesc { get; set; } = string.Empty;

        public DateTime? fechaServer { get; set; } 

        public int? caja_Valida_Concepto { get; set; }

        public int? control { get; set; }

        public int? iva_porc { get; set; }
    }

    public class CajasCrdOperacionTransacData
    {
        public decimal id_seq { get; set; }
        public int num_cuota { get; set; }
        public int fecha_proceso { get; set; }
        public DateTime fecha_pago { get; set; }
        public decimal cuota { get; set; }
        public int mora_dias { get; set; }
        public decimal intcor { get; set; }
        public decimal intmor { get; set; }
        public decimal principal { get; set; }
        public decimal cargos { get; set; }
        public decimal poliza { get; set; }
        public decimal iva { get; set; }
        public int dias_calculo { get; set; }
        public DateTime fecha_corte { get; set; }
    }

    public class CajasCrdAbonoTipoRequest
    {
        public int operacion_id { get; set; }
        public DateTime fecha_cancelacion { get; set; }
    }

    public class CajasCrdAbonosInfoCancelacionData
    {
        public decimal intcor { get; set; }
        public decimal intmor { get; set; }
        public decimal cargos { get; set; }
        public decimal principal { get; set; }
        public decimal cargoanticipo { get; set; }
        public decimal cuota { get; set; }
        public decimal poliza { get; set; }
        public decimal iva { get; set; }
    }

    public class CajasCrdAbonosCtPRegistrarAbonoRequest
    {
        public string mcaja { get; set; } = "";
        public int mapertura { get; set; } = 0;
        public int msesionid { get; set; } = 0;
        public string mtiquete { get; set; } = "";
        public string munidad { get; set; } = "";
        public int operacionid { get; set; } = 0;

        public string tipodoc { get; set; } = "";

        public DateTime? fechacancelacion { get; set; }

        public bool fechacancelacion_enabled { get; set; } = false;

        public decimal totalcajas { get; set; } = 0;
        public decimal totalcancela { get; set; } = 0;
        public decimal diferencia { get; set; } = 0;
        public decimal datosanticipo { get; set; } = 0;
        public decimal datosinteres { get; set; } = 0;
        public decimal datosamortiza { get; set; } = 0;
        public decimal iva { get; set; } = 0;
        public decimal totalpagar { get; set; } = 0;
        public decimal control { get; set; } = 0;
        public string proceso { get; set; } = "";
        public int plazo { get; set; } = 0;

        public int diasactivo { get; set; } = 0;
        public bool recalculacuota { get; set; } = false;

        public TipoAbono tipoabono { get; set; } = TipoAbono.Ordinario;
        public bool diferenciaaplenabled { get; set; } = false;
        public string diferenciaapltexto { get; set; } = "";

        public string usuario { get; set; } = "";
        public string oficinaunidad { get; set; } = "";
        public string oficinacentrocosto { get; set; } = "";
        public string divisa { get; set; } = "";
        public string cedula { get; set; } = "";
        public string nombre { get; set; } = "";
        public string codigo { get; set; } = "";
        public string descripcion { get; set; } = "";
        public string notas { get; set; } = "";
        public decimal saldo_anterior { get; set; } = 0;
        public decimal saldo_nuevo { get; set; } = 0;
        public bool retencion { get; set; } = false;

        public bool factura_visible { get; set; } = false;
        public string tiquete_electronico { get; set; } = "";
        public bool recibo_digital { get; set; } = false;
    }

    public enum TipoAbono
    {
        Ordinario = 0,
        Extraordinario = 1,
        Cancelacion = 2,
        AdelantoCuotas = 3
    }

    public class CajasCrdAbonosCtPRegistrarAbonoResponse
    {
        public bool extraordinario { get; set; } = false;
        public string mensaje { get; set; } = "";
        public string tipodoc { get; set; } = "";
        public string numdoc { get; set; } = "";
    }

    public class CajasCrdInfoExtraordinarioData
    {
        public int dias { get; set; } = 0;
        public decimal intereses { get; set; } = 0;
        public decimal principal { get; set; } = 0;
        public decimal cargos { get; set; } = 0;
    }

    public class CajasCrdDocumentoAfectacionData
    {
        public decimal intcor { get; set; } = 0;
        public decimal intmor { get; set; } = 0;
        public decimal principal { get; set; } = 0;
        public decimal cargos { get; set; } = 0;
        public decimal polizas { get; set; } = 0;
        public decimal iva { get; set; } = 0;
    }

    public class CajasCrdOperacionProxPagoData
    {
        public DateTime? fecha_pago { get; set; }
        public int? num_cuota { get; set; }
        public decimal? cuota { get; set; }
        public string notas { get; set; } = "";
    }

    public class CajasCrdDocAfectacionCargoRow
    {
        public decimal? mov_monto { get; set; }
        public string cod_unidad { get; set; } = "";
        public string cod_centro_costo { get; set; } = "";
        public string cod_cuenta { get; set; } = "";
        public int id_solicitud { get; set; }
        public string codigo { get; set; } = "";
    }

    public class CajasCrdDocAfectacionPolizaRow
    {
        public decimal mov_monto { get; set; } = 0;
        public string cod_cuenta { get; set; } = "";
    }

    public class CajasCrdOperacionCtasData
    {
        public string cod_divisa { get; set; } = "COL";
        public string cod_unidad { get; set; } = "";
        public string cod_centro_costo { get; set; } = "";

        public string ctaintc { get; set; } = "";
        public string ctaintm { get; set; } = "";
        public string ctaiva { get; set; } = "";
        public string ctaamortiza { get; set; } = "";

        public int id_solicitud { get; set; } = 0;
        public string codigo { get; set; } = "";
    }
}
