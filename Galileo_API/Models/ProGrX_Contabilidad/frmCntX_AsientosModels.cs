namespace Galileo_API.Models.ProGrX_Contabilidad
{
    public class CntXAsientoData
    {
        public string tipo_asiento { get; set; } = "";
        public int cod_contabilidad { get; set; } = 0;
        public string num_asiento { get; set; } = "";
        public int anio { get; set; } = 0;
        public int mes { get; set; } = 0;
        public DateTime? fecha_asiento { get; set; } 
        public DateTime? fecha_aplicado { get; set; }
        public string descripcion { get; set; } = "";
        public string notas { get; set; } = "";
        public string? referencia { get; set; }
        public string? user_crea { get; set; }
        public string? user_modifica { get; set; }
        public string? user_aplica { get; set; }
        public string? user_autoriza { get; set; }
        public int modulo { get; set; } = 0;
        public byte[]? ts { get; set; }
    }

    public class CntXAsientoDetalleData
    {
        public string cod_cuenta { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string documento { get; set; } = string.Empty;
        public string? detalle { get; set; }

        public decimal monto_debito { get; set; } = 0;
        public decimal monto_credito { get; set; } = 0;

        public int num_linea { get; set; } = 0;

        public decimal saldo_inicial { get; set; } = 0;
        public decimal total_debitos { get; set; } = 0;
        public decimal total_creditos { get; set; } = 0;

        public string cod_unidad { get; set; } = string.Empty;
        public string unides { get; set; } = string.Empty;

        public string cod_divisa { get; set; } = string.Empty;
        public string divisa { get; set; } = string.Empty;

        public decimal tc { get; set; } = 0;
        public decimal tc_ajuste { get; set; } = 0;

        public string cod_centro_costo { get; set; } = string.Empty;
        public string? centrocosto { get; set; }
    }

    public class CntXAsientoGuardarRequest
    {
        public CntXAsientoData asiento { get; set; } = new();
        public List<CntXAsientoDetalleData> detalle { get; set; } = new();
        public bool balanceado { get; set; } = false;
    }

    public class CntXAsientoCopiarRequest
    {
        public int cod_contabilidad { get; set; } = 0;
        public string tipo_asiento { get; set; } = string.Empty;
        public string num_asiento { get; set; } = string.Empty;
        public string nuevo_num_asiento { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public DateTime? fecha { get; set; }
        public string notas { get; set; } = string.Empty;
        public bool copiar_detalles { get; set; } = false;
        public string documento { get; set; } = string.Empty;
        public string detalle { get; set; } = string.Empty;
        public string referencia { get; set; } = string.Empty;
        public bool as_reversion { get; set; } = false;
    }
}
