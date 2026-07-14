namespace Galileo_API.Models.ProGrX.Creditos
{
    public class CrConsultaOperacionesBusquedaOperacionDto
    {
        public int id_solicitud { get; set; } = 0;
        public string codigo { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
    }

    public class CrConsultaOperacionesBusquedaSocioDto
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
    }

    public class CrConsultaOperacionesListaDto
    {
        public int id_solicitud { get; set; } = 0;
        public string codigo { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public DateTime? fechasol { get; set; }
        public decimal montosol { get; set; } = 0;
        public string estadosol { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string proceso { get; set; } = string.Empty;
        public string estadosol_desc { get; set; } = string.Empty;
        public string estado_desc { get; set; } = string.Empty;
        public string proceso_desc { get; set; } = string.Empty;
    }

    public class CrConsultaOperacionesDetalleDto
    {
        public int operacion { get; set; } = 0;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string detalle { get; set; } = string.Empty;
    }

    internal sealed class CrConsultaOperacionesMainData
    {
        public int id_solicitud { get; set; } = 0;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string codigo { get; set; } = string.Empty;
        public string desccod { get; set; } = string.Empty;
        public string garantia { get; set; } = string.Empty;
        public string estadosol { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string userrec { get; set; } = string.Empty;
        public DateTime? fechasol { get; set; }
        public decimal montosol { get; set; } = 0;
        public decimal plazo { get; set; } = 0;
        public decimal @int { get; set; } = 0;
        public decimal cuota { get; set; } = 0;
        public string userres { get; set; } = string.Empty;
        public DateTime? fechares { get; set; }
        public decimal montoapr { get; set; } = 0;
        public string userfor { get; set; } = string.Empty;
        public DateTime? fechaforp { get; set; }
        public DateTime? fecha_calculo_int { get; set; }
        public decimal monto_girado { get; set; } = 0;
        public string tdocumento { get; set; } = string.Empty;
        public string ndocumento { get; set; } = string.Empty;
        public string documento_referido { get; set; } = string.Empty;
        public int id_comite { get; set; } = 0;
        public string acta { get; set; } = string.Empty;
        public string observacion { get; set; } = string.Empty;
    }

    internal sealed class CrConsultaOperacionesFiadorData
    {
        public string cedulaf { get; set; } = string.Empty;
        public string nomb { get; set; } = string.Empty;
    }

    internal sealed class CrConsultaOperacionesRefundicionCarteraData
    {
        public int id_solicitud { get; set; } = 0;
        public string codigo { get; set; } = string.Empty;
        public decimal monto { get; set; } = 0;
        public decimal intcor { get; set; } = 0;
        public decimal intmor { get; set; } = 0;
    }

    internal sealed class CrConsultaOperacionesRefundicionRetencionData
    {
        public int id_solicitud { get; set; } = 0;
        public string codigo { get; set; } = string.Empty;
        public decimal monto { get; set; } = 0;
        public decimal mora { get; set; } = 0;
    }

    internal sealed class CrConsultaOperacionesDesembolsoData
    {
        public decimal monto { get; set; } = 0;
        public string cuenta_conta { get; set; } = string.Empty;
        public string concepto { get; set; } = string.Empty;
    }

    internal sealed class CrConsultaOperacionesTesTransaccionData
    {
        public int nsolicitud { get; set; } = 0;
    }

    internal sealed class CrConsultaOperacionesTesAsientoData
    {
        public string cuenta_contable { get; set; } = string.Empty;
        public decimal monto { get; set; } = 0;
        public string debehaber { get; set; } = string.Empty;
    }

    internal sealed class CrConsultaOperacionesAseAsientoData
    {
        public string recas_cuenta { get; set; } = string.Empty;
        public decimal recas_monto { get; set; } = 0;
        public string recas_debehaber { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    internal sealed class CrConsultaOperacionesTmpAsientoData
    {
        public string tmp_cuenta { get; set; } = string.Empty;
        public decimal tmp_monto { get; set; } = 0;
        public string tmp_debehaber { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    internal sealed class CrConsultaOperacionesDescripcionData
    {
        public string describe { get; set; } = string.Empty;
    }
}