namespace Galileo.Models.ProGrX.Fondos
{
    public class FndPagoComisionFiltros
    {
        public Nullable<DateTime> fecha_inicio { get; set; }
        public Nullable<DateTime> fecha_corte { get; set; }
        public int id_banco { get; set; }
        public Nullable<DateTime> fecha_generacion { get; set; }
        public string usuario { get; set; } = string.Empty;
    }

    public class FndPagoComisionVendedorData
    {
        public string cod_vendedor { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string cuenta_ahorros { get; set; } = string.Empty;
        public string tipo_pago { get; set; } = string.Empty;
        public int? cod_banco { get; set; }
        public decimal? minimo { get; set; }
        public decimal? porc_comision { get; set; }
        public decimal? monto { get; set; }
        public int? casos { get; set; }
        public decimal? monto_comision { get; set; }
    }

    public class MaestroTesoreriaRequest
    {
        public string tipoDocumento { get; set; } = string.Empty;
        public long banco { get; set; } = 0;
        public decimal? monto { get; set; }
        public string codigo { get; set; } = string.Empty;
        public string beneficiario { get; set; } = string.Empty;
        public long? op { get; set; }
        public string detalle1 { get; set; } = string.Empty;
        public long? referencia { get; set; }
        public string detalle2 { get; set; } = string.Empty;
        public string cuenta { get; set; } = string.Empty;
        public DateTime? fecha { get; set; }
    }
}
