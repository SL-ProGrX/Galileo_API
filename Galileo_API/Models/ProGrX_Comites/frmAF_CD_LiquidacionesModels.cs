using Galileo.Models.ERROR;

namespace Galileo_API.Models.ProGrX_Comites
{
    public class AfCdOperacionData
    {
        public int noperacion { get; set; } = 0;
        public DateTime? activa_fecha { get; set; }
        public int dias_pendientes { get; set; } = 0;
        public decimal monto { get; set; } = 0;
        public string actividad { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string desembolso { get; set; } = string.Empty;
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime? fecha_emision { get; set; }
    }

    public class AfCdOperacionHistoricoData
    {
        public int operacion { get; set; } = 0;
        public string notas { get; set; } = string.Empty;
        public DateTime? liquida_fecha { get; set; }
        public DateTime? activa_fecha { get; set; }
        public DateTime? fecha_emision { get; set; }
        public decimal monto { get; set; } = 0;
        public string actividad { get; set; } = string.Empty;
        public DateTime? tesoreria_fecha { get; set; }
        public int? tesoreria_nsolicitud { get; set; }
        public string estado { get; set; } = string.Empty;
        public string aprueba { get; set; } = string.Empty;
        public string desembolso { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public string tesoreria_beneficiario { get; set; } = string.Empty;
        public string tesoreria_codigo { get; set; } = string.Empty;
    }

    public class AfCdFacturaData
    {
        public int noperacion { get; set; } = 0;
        public bool deposito { get; set; } = false;
        public string ndocumento { get; set; } = string.Empty;
        public DateTime? fecha_documento { get; set; }
        public string detalle { get; set; } = string.Empty;
        public decimal monto { get; set; } = 0;
    }

    public class AfCdDetalleLiquidacionMontosData
    {
        public decimal total { get; set; }
        public decimal totalFactura { get; set; }
        public decimal diferencia { get; set; }
    }
}
