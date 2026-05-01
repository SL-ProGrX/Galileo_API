namespace Galileo_API.Models.ProGrX.Cobros
{
    public class CbrIncobrableListaItem
    {
        public int? id_solicitud { get; set; }
        public int? cod_incobrable { get; set; }
        public string codigo { get; set; } = string.Empty;
        public int? cxc_operacion { get; set; }
        public decimal? saldo { get; set; }
        public decimal? recaudado { get; set; }
        public DateTime? registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public string tipo_documento { get; set; } = string.Empty;
        public string cod_transaccion { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string notas_registro { get; set; } = string.Empty;
    }

    public class CbrIncobrableMovimientoItem
    {
        public int? crd_operacion { get; set; }
        public string cod_concepto { get; set; } = string.Empty;
        public int? operacion { get; set; }
        public decimal? saldo_inicial { get; set; }
        public decimal? mov_principal { get; set; }
        public decimal? saldo_final { get; set; }
        public DateTime? registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public string cod_caja { get; set; } = string.Empty;
        public string tipo_documento { get; set; } = string.Empty;
        public string num_documento { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }
}
