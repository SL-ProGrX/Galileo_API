namespace Galileo_API.Models.ProGrX.Creditos
{
    public class CrConsultaBitacoraRequest
    {
        public string cedula { get; set; } = string.Empty;
        public DateTime? fecha_inicio { get; set; }
        public DateTime? fecha_corte { get; set; }
        public bool? mov_bancario { get; set; }
    }

    public class CrConsultaBitacoraLista<T>
    {
        public int total { get; set; }
        public List<T> lista { get; set; } = new();
    }

    public class CrConsultaBitacoraEncabezadoDto
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public DateTime? fecha_servidor { get; set; }
        public DateTime? fecha_inicio { get; set; }
        public DateTime? fecha_corte { get; set; }
    }

    public class CrConsultaBitacoraRegistroDto
    {
        public string ntransaccion { get; set; } = string.Empty;
        public string tdocumento { get; set; } = string.Empty;
        public string ndocumento { get; set; } = string.Empty;
        public DateTime? fecha { get; set; }
        public string usuario { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public string concepto { get; set; } = string.Empty;
        public string detalle { get; set; } = string.Empty;
        public string referencia { get; set; } = string.Empty;
        public string codapp { get; set; } = string.Empty;
    }

    public class CrConsultaBitacoraCreditosDto
    {
        public long id_solicitud { get; set; }
        public string codigo { get; set; } = string.Empty;
        public string lineax { get; set; } = string.Empty;
        public DateTime? fecha { get; set; }
        public decimal intcor { get; set; }
        public decimal intmor { get; set; }
        public decimal cargo { get; set; }
        public decimal poliza { get; set; }
        public decimal principal { get; set; }
        public string ncon { get; set; } = string.Empty;
        public string tcon { get; set; } = string.Empty;
        public string proceso { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string fuente { get; set; } = string.Empty;
        public string concepto { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public string cod_caja { get; set; } = string.Empty;
        public string cod_concepto { get; set; } = string.Empty;
        public string garantia { get; set; } = string.Empty;
        public string garantiadesc { get; set; } = string.Empty;
        public decimal total_mov { get; set; }
        public string antiguedad { get; set; } = string.Empty;
        public decimal tasa_mov { get; set; }
        public decimal tasa_actual { get; set; }
    }

    public class CrConsultaBitacoraFondosDto
    {
        public int cod_operadora { get; set; }
        public string cod_plan { get; set; } = string.Empty;
        public long cod_contrato { get; set; }
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string plan { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public DateTime? fecha { get; set; }
        public string usuario { get; set; } = string.Empty;
        public string conceptodesc { get; set; } = string.Empty;
        public string tipodocdesc { get; set; } = string.Empty;
        public string ncon { get; set; } = string.Empty;
        public string cod_caja { get; set; } = string.Empty;
    }

    public class CrConsultaBitacoraPatrimonioDto
    {
        public string plan { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public DateTime? fecha { get; set; }
        public string usuario { get; set; } = string.Empty;
        public string conceptodesc { get; set; } = string.Empty;
        public string tipodocdesc { get; set; } = string.Empty;
        public string ncon { get; set; } = string.Empty;
        public string cod_caja { get; set; } = string.Empty;
    }

    public class CrConsultaBitacoraBancosDto
    {
        public string bancodesc { get; set; } = string.Empty;
        public string ctaid { get; set; } = string.Empty;
        public string ctadesc { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public DateTime? fecha { get; set; }
        public string usuario { get; set; } = string.Empty;
        public string cod_divisa { get; set; } = string.Empty;
        public string ref_01 { get; set; } = string.Empty;
        public string ref_02 { get; set; } = string.Empty;
        public string ref_03 { get; set; } = string.Empty;
        public string conceptodesc { get; set; } = string.Empty;
        public string tipodoc { get; set; } = string.Empty;
        public string detalle { get; set; } = string.Empty;
        public long nsolicitud { get; set; }
        public string ndocumento { get; set; } = string.Empty;
        public string documento_base { get; set; } = string.Empty;
        public string estadodesc { get; set; } = string.Empty;
        public DateTime? fecha_anula { get; set; }
        public DateTime? fecha_solicitud { get; set; }
        public DateTime? fecha_emision { get; set; }
    }
}