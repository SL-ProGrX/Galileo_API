namespace Galileo_API.Models.ProGrX.Cobros
{
    public class FrmCOReversionCobroJudicialModels
    {

        public class CrdReversionCobroJudicialConsultaResponse
        {
            public int? Operacion { get; set; }
            public int? PlazoRestante { get; set; }

            public string Cedula { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
            public string Divisa { get; set; } = string.Empty;

            public decimal Tasa { get; set; }
            public decimal TasaOriginal { get; set; }

            public decimal Saldo { get; set; }
            public decimal InteresesCorte { get; set; }
            public decimal Intereses { get; set; }
            public decimal Amortizacion { get; set; }
            public decimal Cargos { get; set; }
            public decimal Poliza { get; set; }
            public decimal Honorarios { get; set; }

            public decimal TotalAtrasado { get; set; }
            public decimal Total { get; set; }

            public int PlazoOriginal { get; set; }

            public string Codigo { get; set; } = string.Empty;
            public string Descripcion { get; set; } = string.Empty;

            public bool Opex { get; set; }
            public string OpexDescripcion { get; set; } = "No";

            public string ProcesoCodigo { get; set; } = string.Empty;
            public string ProcesoDescripcion { get; set; } = string.Empty;

            public bool PermiteReversar { get; set; }

        
        }
        public class CrdReversionCobroJudicialConsultaDbModel
        {
            public string Cedula { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
            public decimal Saldo { get; set; }
            public string Proceso { get; set; } = string.Empty;
            public decimal Tasa { get; set; }
            public int Plazo { get; set; }
            public decimal TasaOriginal { get; set; }
            public decimal Intereses { get; set; }
            public string Codigo { get; set; } = string.Empty;
            public string Descripcion { get; set; } = string.Empty;
            public int? Opex { get; set; }
            public decimal MoraIntC { get; set; }
            public decimal MoraIntM { get; set; }
            public decimal MoraAmortiza { get; set; }
            public decimal Cargos { get; set; }
            public decimal Poliza { get; set; }
            public string Cod_Divisa { get; set; } = string.Empty; 
        }
        public class CrdReversionCobroJudicialInteresesHoyDbModel
        {
            public decimal RegIntCor { get; set; }
            public decimal RegIntMor { get; set; }
            public decimal RegPrincipal { get; set; }
        }

        public class CrdReversionCobroJudicialReversaRequest
        {
            public int? Operacion { get; set; }
            public string Notas { get; set; } = string.Empty;
            public string Usuario { get; set; } = string.Empty;
        }
        public class CrdReversionCobroJudicialReversaResponse
        {
            public bool Reversada { get; set; }
            public string TipoDocumento { get; set; } = string.Empty;
            public string NumeroDocumento { get; set; } = string.Empty;
            public string Mensaje { get; set; } = string.Empty;
        }

        public class CrdReversionCobroJudicialReversaDbModel
        {
            public int? Pass { get; set; }
            public string NumDoc { get; set; } = string.Empty;
            public string TipoDoc { get; set; } = string.Empty;
            public string Mensaje { get; set; } = string.Empty;
        }

    }
}
