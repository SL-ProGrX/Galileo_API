namespace PgxAPI.Models.GEN
{
    public class CcCaGenericData
    {
        public string idx { get; set; } = string.Empty;
        public string itmx { get; set; } = string.Empty;
    }

    public class PrmCaRemesaDt
    {
        public string codRemesa { get; set; } = string.Empty;
        public int numLinea { get; set; }
        public string tarjeta { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string autorizacion { get; set; } = string.Empty;
        public DateTime fechaProcesa { get; set; }
        public decimal monto { get; set; }
        public decimal comision { get; set; }
        public string estado { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public bool detallado { get; set; }
        public string nombre { get; set; } = string.Empty;
    }

    public class PrmCaRemesa
    {
        public int cod_remesa { get; set; }
        public DateTime fecha { get; set; }
        public string usuario { get; set; } = string.Empty;
        public string proceso { get; set; } = string.Empty;
        public DateTime fecha_inicio { get; set; }
        public DateTime fecha_corte { get; set; }
        public string notas { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public DateTime? tesoreria_fecha { get; set; }
        public string tesoreria_usuario { get; set; } = string.Empty;
        public string tesoreria_solicitud { get; set; } = string.Empty;
        public string tipo_documento { get; set; } = string.Empty;
        public string cod_transaccion { get; set; } = string.Empty;
        public string cod_linea { get; set; } = string.Empty;
        public string cod_entidad { get; set; } = string.Empty;
        public int num_cuotas { get; set; }
        public int idx { get; set; }
        public string itmx { get; set; } = string.Empty;
    }

    public class FiltrosBuscarCasos
    {
        public int proceso { get; set; }
        public string linea { get; set; } = string.Empty;
        public bool soloTarjetasValidas { get; set; }
        public string fechaCorte { get; set; } = string.Empty;
        public int nCuotas { get; set; }
    }

    public class CcCaCasosData
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public decimal compromiso { get; set; }
        public string? tarjeta_numero { get; set; }
        public DateTime? tarjeta_vence { get; set; }
    }

    public class RemesaInsert
    {
        public int proceso { get; set; }
        public string linea { get; set; } = string.Empty;
        public string entidad { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public string fechacorte { get; set; } = string.Empty;
        public int ncuotas { get; set; }
    }

    public class RemesaDetalleInsert
    {
        public int remesa { get; set; }
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public decimal compromiso { get; set; }
        public string tarjeta { get; set; } = string.Empty;
        public DateTime tarjetavence { get; set; }
    }

    public class RemesaArchivoData
    {
        public string numero_afiliado { get; set; } = string.Empty;
        public int cod_remesa { get; set; }
        public string proceso { get; set; } = string.Empty;
        public DateTime fecha_transaccion { get; set; }
        public DateTime fecha_vence { get; set; }
        public string tarjeta { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public string nombre { get; set; } = string.Empty;
        public string email_report { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string tarjeta_vence { get; set; } = string.Empty;
        public string tarjeta_vence_mask { get; set; } = string.Empty;
        public string referencia { get; set; } = string.Empty;
        public int cargo_id { get; set; }
        public string formato { get; set; } = string.Empty;
    }

    public class RemesaAutorizacion
    {
        public int codremesa { get; set; }
        public string tarjeta { get; set; } = string.Empty;
        public string autorizacion { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public decimal comision { get; set; }
        public DateTime fecha { get; set; }
        public string referencia { get; set; } = string.Empty;
    }

    public class CaRemesaAplicaInicializa
    {
        public string tipodoc { get; set; } = string.Empty;
        public string numdoc { get; set; } = string.Empty;
        public int proceso { get; set; }
        public string rlinea { get; set; } = string.Empty;
    }

    public class CaAbonosDetallaMain
    {
        public int total { get; set; }
        public int pendientes { get; set; }
        public int procesados { get; set; }
    }
}
