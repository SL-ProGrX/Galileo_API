namespace Galileo_API.Models.ProGrX_Polizas
{
    public class FrmCrPolizaProcRecepcionModels
    {
        public class PolizaAseguradoraCorte
        {
            public int? Pass { get; set; }
            public string Mensaje { get; set; } = string.Empty;
            public string Movimiento { get; set; } = string.Empty;
            public int? IdFactura { get; set; }
        }
        public class PolizaAseguradoraCorteData
        {
            public DateTime? Corte { get; set; }
            public string CodPoliza { get; set; } = string.Empty;
            public int? IdFactura { get; set; }
            public string AseguradoraId { get; set; } = string.Empty;
            public int? ProveedorId { get; set; }
            public string Factura { get; set; } = string.Empty;
            public string FormaPago { get; set; } = string.Empty;
            public DateTime? Vence { get; set; }
            public string Divisa { get; set; } = string.Empty;
            public string Unidad { get; set; } = string.Empty;
            public string CentroCosto { get; set; } = string.Empty;
            public float TipoCambio { get; set; }
            public string Notas { get; set; } = string.Empty; 
        }
        public class PolizaAseguradoraCorteDetalleData
        {
            public int? IdFactura { get; set; }
            public int? IdLinea { get; set; } 
            public string Cedula { get; set; } = string.Empty; 
            public string Nombre { get; set; } = string.Empty; 
            public string NumPoliza { get; set; } = string.Empty;
            public decimal? MontoAsegurado { get; set; }
            public decimal? Prima { get; set; }
            public int? Operacion { get; set; }
            public int? Inicializa { get; set; }
        }

        public class PolizaAseguradoraCorteDetalleConsulta
        {
            public int? Id_Solicitud { get; set; }
            public string N_Poliza { get; set; } = string.Empty;   
            public string Cedula { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty; 
            public decimal? Monto_Asegurado { get; set; } 
            public decimal? Prima { get; set; } 
          
        }

        public class PolizaDatos
        {
            public int? IdFactura { get; set; }
            public string Cod_Aseguradora { get; set; } = string.Empty;
            public string Aseguradora_Desc { get; set; } = string.Empty;
            public string Cod_Proveedor { get; set; } = string.Empty;
            public string Proveedor_Desc { get; set; } = string.Empty;
            public string Unidad_Desc { get; set; } = string.Empty;
            public string Cod_Unidad { get; set; } = string.Empty;
            public string Cod_Centro_Costo { get; set; } = string.Empty;
            public string Centro_Costo_Desc { get; set; } = string.Empty;
            public DateTime? @Corte { get; set; }
            public int? Inicializa { get; set; }
        }

        public  class AppendExecArgs
        {
            public string Suf { get; set; } = string.Empty;
            public int? IdFactura { get; set; } = 0;
            public int? Linea { get; set; } = 0;
            public string Cedula { get; set; } = string.Empty;
            public PolizaAseguradoraCorteDetalleData Row { get; init; } = default!;
            public string Usuario { get; set; } = string.Empty;
            public int Inicializa { get; init; } = 0;
        }

    }
}
