namespace Galileo_API.Models.ProGrX_Polizas
{
    public class FrmPolizaFacturaVerModels
    {


        public class CrdPolizaFacturaVerFacturaResponse
        {
            public string? Factura { get; set; } = string.Empty;
            public int? ProveedorCodigo { get; set; }

            public string? ProveedorNombre { get; set; } = string.Empty;

            public string? Estado { get; set; } = string.Empty;

            public DateTime? Fecha { get; set; }

            public DateTime? FechaVence { get; set; }

            public string DivisaFactura { get; set; } = string.Empty;

            public string DivisaProveedor { get; set; } = string.Empty;

            public decimal? TipoCambio { get; set; }

            public decimal? Total { get; set; }

            public decimal? TotalDivisaReal { get; set; }

            public decimal? Impuesto { get; set; }

            public decimal? Saldo { get; set; }

            public string FormaPago { get; set; } = string.Empty;
            public string CxP_Estado { get; set; } = string.Empty;
            public string Notas { get; set; } = string.Empty;
            public string Creacion_User { get; set; } = string.Empty;        
            public DateTime? Creacion_Fecha { get; set; }
            public string anula_user { get; set; } = string.Empty;
            public DateTime? anula_fecha { get; set; }
        }
        public class CrdPolizaFacturaVerAsientoModel
        {
            public string CuentaMask { get; set; } = string.Empty;

            public string CuentaDescripcion { get; set; } = string.Empty;

            public string CodUnidad { get; set; } = string.Empty;
            public string CodCentroCosto { get; set; } = string.Empty;
            public string? Unidad { get; set; }

            public string? CentroCosto { get; set; }

            public string? Divisa { get; set; } = string.Empty;
            public string CodDivisa { get; set; } = string.Empty;

            public decimal TipoCambio { get; set; } = 0;
            
            public decimal Debito { get; set; } = 0;

            public decimal Credito { get; set; } = 0;
        }

        public class CrdPolizaFacturaVerTotalesModel
        {
            public decimal Debito { get; set; } = 0;

            public decimal Credito { get; set; } = 0;

            public decimal Diferencia { get; set; } = 0;
        }
        public class CrdPolizaFacturaVerAsientosResponse
        {
            public List<CrdPolizaFacturaVerAsientoModel> Lineas { get; set; } = [];

            public CrdPolizaFacturaVerTotalesModel Totales { get; set; } = new();
        }

        public class CrdPolizaFacturaVerDivisaLocalModel
        {
            public string Divisa { get; set; } = string.Empty;

            public string DivisaLocal { get; set; } = string.Empty;
        }

    }
}
