namespace Galileo_API.Models.ProGrX.Cajas
{
    public class CajasCierreCuentasData
    {
        public int id_banco { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public string cta { get; set; } = string.Empty;
    }

    public class CajasCierreFormaPagoData
    {
        public string cod_forma_pago { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string cod_cuenta { get; set; } = string.Empty;
        public string cod_divisa { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public bool efectivo { get; set; }
        public decimal monto { get; set; }
        public decimal importe_funcional { get; set; }
    }

    public class CajasCierreDenominacionData
    {
        public string tipo { get; set; } = string.Empty;
        public decimal denominacion { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public int cantidad { get; set; }
        public decimal monto { get; set; }
    }

    public class CajasCierreDepositosData
    {
        public int linea { get; set; }
        public int cod_apertura { get; set; } 
        public string cod_caja { get; set; } = string.Empty;
        public string cod_divisa { get; set; } = string.Empty;
        public string dp_numero { get; set; } = string.Empty;
        public string dp_cuenta { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public DateTime registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public int id_banco { get; set; }
        public bool estado { get; set; } 
        public string banco_estado { get; set; } = string.Empty;
        public string banco_numero { get; set; } = string.Empty;
        public int tesoreria_id { get; set; }
        public DateTime? tesoreria_fecha { get; set; }
        public string tesoreria_usuario { get; set; } = string.Empty;
        public string bancodesc { get; set; } = string.Empty;
        public int idx { get; set; }
        public string itmx { get; set; } = string.Empty;
    }

}
