namespace Galileo.Models.CxP
{
    public class CxpAnticiposFiltros
    {
        public int proveedor { get; set; }
        public string cargoCod { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public string divisa { get; set; } = string.Empty;
        public string documento { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public string fechaCargo { get; set; } = string.Empty;
    }

    public class CargoDto
    {
        public int COD_CARGO { get; set; }
        public string DESCRIPCION { get; set; } = string.Empty;
        public decimal MONTO { get; set; }
    }

    public class AdelantoRegistradoDto
    {
        public int idx { get; set; }
        public int cod_proveedor { get; set; }
        public int cod_cargo { get; set; }
        public int id_cargo { get; set; }
        public string notas { get; set; } = string.Empty;
        public DateTime fecha { get; set; }
        public string usuario { get; set; } = string.Empty;
        public string documento { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public int cod_orden { get; set; }
        public string anticipos { get; set; } = string.Empty;
        public DateTime fecha_cobro_anticipo { get; set; }
        public string tesoreria { get; set; } = string.Empty;
        public DateTime fecha_vencimiento { get; set; }
        public string cargo { get; set; } = string.Empty;
        public decimal saldo { get; set; }
    }

    public class HistorialPagoDto
    {
        public string anticipos { get; set; } = string.Empty;
        public int cod_proveedor { get; set; }
        public string cod_factura { get; set; } = string.Empty;
        public DateTime registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public string cod_divisa { get; set; } = string.Empty;
        public decimal tipo_cambio { get; set; }
        public int npago { get; set; }
    }
}