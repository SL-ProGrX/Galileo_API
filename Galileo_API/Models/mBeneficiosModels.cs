namespace Galileo.Models
{
    public class BitacoraBeneInsertarDto
    {
        public int EmpresaId { get; set; }
        public int id_bitacora { get; set; }
        public string cod_beneficio { get; set; } = string.Empty;
        public long? consec { get; set; }
        public string movimiento { get; set; } = string.Empty;
        public string detalle { get; set; } = string.Empty;
        public string registro_usuario { get; set; } = string.Empty;
    }

    public class ValidacionRow
    {
        public int COD_VAL { get; set; }
        public int PRIORIDAD { get; set; }
        public string? query_val { get; set; }
        public int resultado_val { get; set; }
        public string? msj_val { get; set; }

        // Estas dos solo vienen cuando la query las incluye.
        // Si no vienen, Dapper las deja en false (valor por defecto).
        public bool registro_justifica { get; set; }
        public bool pago_justifica { get; set; }
    }
}
