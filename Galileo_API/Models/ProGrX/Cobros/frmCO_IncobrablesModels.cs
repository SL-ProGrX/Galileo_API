namespace Galileo_API.Models.ProGrX.Cobros
{
    public class FrmCOIncobrablesModels
    {

        public class CrdIncobrableDetalleResponse
        {
            public bool? ExisteIncobrable { get; set; }
            public bool? MostrarTabRegistro { get; set; }
            public bool? MostrarTabReversion { get; set; }
            public bool? IncobrableActivo { get; set; }

            public int? IdSolicitud { get; set; }
            public int? CodIncobrable { get; set; }

            public string Cedula { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
            public string Codigo { get; set; } = string.Empty;
            public string Descripcion { get; set; } = string.Empty;
            public string Divisa { get; set; } = string.Empty;
            public string Garantia { get; set; } = string.Empty;
            public string Opex { get; set; } = string.Empty;

            public decimal Saldo { get; set; }=0;   
            public decimal IntCor { get; set; } = 0;
            public decimal IntMor { get; set; } = 0;
            public decimal Amortizacion { get; set; } = 0;
            public decimal Cargos { get; set; } = 0;
            public decimal Poliza { get; set; } = 0;


            public string NotasRegistro { get; set; } = string.Empty;
            public string RegistroUsuario { get; set; } = string.Empty;
            public string RegistroDocumento { get; set; } = string.Empty;
            public string RegistroFecha { get; set; } = string.Empty;

            public string NotasReversion { get; set; } = string.Empty;
            public string ReversionUsuario { get; set; } = string.Empty;
            public string ReversionDocumento { get; set; } = string.Empty;
            public string ReversionFecha { get; set; } = string.Empty;
            public decimal? ReversionRecargo { get; set; }

            public string Proceso { get; set; } = string.Empty;
            public decimal? TotalMora { get; set; }
            public decimal? TotalMoraLegal { get; set; }
            public decimal? TotalAtrasado { get; set; }
            public string EstadoMoroso { get; set; } = string.Empty;
            public string Estado { get; set; } = string.Empty;
            public DateTime FechaServer { get; set; }
            public decimal InteresV { get; set; } = 0;
            public int PriDeduc { get; set; } = 0;
            public int FecUlt { get; set; } = 0;
            public DateTime FechaActual { get; set; }
        }

        public class CrdIncobrableAccionResponse
        {
            public string Mensaje { get; set; } = string.Empty;
            public string TipoDoc { get; set; } = string.Empty;
            public string NumDoc { get; set; } = string.Empty;
            public bool? GeneroDocumento { get; set; }
        }
        public class CrdIncobrableReversaRequest
        {
            public int? IdSolicitud { get; set; }
            public decimal Recargo { get; set; } = 0;
            public string Usuario { get; set; } = string.Empty;
            public string Notas { get; set; } = string.Empty;
        }
        public class CrdIncobrableAplicarRequest
        {
            public int? IdSolicitud { get; set; }
            public string Usuario { get; set; } = string.Empty;
            public string Notas { get; set; } = string.Empty;
        }
        public class CrdIncobrablesConsultaDbModel
        {
            public int? Id_Solicitud { get; set; }
            public string Codigo { get; set; } = string.Empty;
            public string Cedula { get; set; } = string.Empty;
            public DateTime FechaServer { get; set; }
            public decimal? Saldo { get; set; }
            public string Estado { get; set; } = string.Empty;
            public string Proceso { get; set; } = string.Empty;
            public int? Opex { get; set; }
            public string LineaDesc { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
            public string GarantiaDesc { get; set; } = string.Empty;
        }

        public class CrdIncobrablesMoraDbModel
        {
            public decimal RegIntCor { get; set; } = 0;
            public decimal RegIntMor { get; set; }=0; 
            public decimal Poliza { get; set; } = 0;
            public decimal Cargos { get; set; } = 0;
            public decimal RegPrincipal { get; set; } = 0;
            public string Antiguedad { get; set; } = string.Empty;
        }

        public class CrdIncobrableDocumentoDbModel
        {
            public string NumDoc { get; set; } = string.Empty;
            public string TipoDoc { get; set; } = string.Empty;
        }
        public class CalculoInteresSinPlanPagosModel
        {
            public int IdSolicitud { get; set; } = 0;
            public decimal Saldo { get; set; } = 0;
            public decimal InteresV { get; set; } = 0;
            public int PriDeduc { get; set; } = 0;
            public int FecUlt { get; set; } = 0;
            public DateTime FechaActual { get; set; }
            public decimal MoraAmortiza { get; set; } = 0;
            public decimal MoraIntC { get; set; } = 0;
            public decimal MoraIntM { get; set; } = 0;
        }
    }

}

