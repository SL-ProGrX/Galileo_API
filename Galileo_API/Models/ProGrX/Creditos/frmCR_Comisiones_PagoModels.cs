namespace Galileo_API.Models.ProGrX.Creditos
{
    public class FrmCrComisionesPagoModels
    {
        public static class CrdComisionesPagoEstados
        {
            public const string Abierta = "A";
            public const string Cerrada = "C";
            public const string Proceso = "P";
            public const string Trasladada = "T";
        }
        public class BancoDropDownDbModel
        {
            public object? IdX { get; set; }
            public string ItmX { get; set; } = string.Empty;
        }
        public class CrdComisionesPagoRemesaModel
        {
            public int CodRemesa { get; set; } = 0;
            public string Usuario { get; set; } = string.Empty;
            public DateTime Fecha { get; set; }
            public string Estado { get; set; } = string.Empty;
            public string EstadoDescripcion { get; set; } = string.Empty;
            public DateTime? FechaInicio { get; set; }
            public DateTime? FechaCorte { get; set; }
            public string Notas { get; set; } = string.Empty;
            public string CodComision { get; set; } = string.Empty;
            public string ComisionDescripcion { get; set; } = string.Empty;
            public int TesBanco { get; set; } = 0;
            public string BancoDescripcion { get; set; } = string.Empty;
            public string TesTipo { get; set; } = string.Empty;
        }

        public class CrdComisionesPagoRemesaGuardarRequest
        {
            public int? CodRemesa { get; set; }
            public DateTime FechaInicio { get; set; }
            public DateTime FechaCorte { get; set; }
            public string Notas { get; set; } = string.Empty;
            public string CodComision { get; set; } = string.Empty;
            public int TesBanco { get; set; }
            public string TesTipo { get; set; } = string.Empty;
            public string Usuario { get; set; } = string.Empty;
        }

        public class CrdComisionesPagoRemesaGuardarResponse
        {
            public int CodRemesa { get; set; } = 0;
            public bool EsNueva { get; set; }
            public string Mensaje { get; set; } = string.Empty;
        }

        public class CrdComisionesPagoRemesaEliminarRequest
        {
            public int CodRemesa { get; set; } = 0;
            public string Usuario { get; set; } = string.Empty;
        }

        public class CrdComisionesPagoRemesaSelectorModel
        {
            public int CodRemesa { get; set; } = 0;
            public string Usuario { get; set; } = string.Empty;
            public DateTime Fecha { get; set; }
            public DateTime FechaInicio { get; set; }
            public DateTime FechaCorte { get; set; }
            public string Descripcion { get; set; } = string.Empty;
        }

        //Modelos para carga
        public class CrdComisionesPagoPendientesRequest
        {
            public int CodRemesa { get; set; } = 0;
            public string? CodOficina { get; set; }
        }

        public class CrdComisionesPagoPendienteModel
        {
            public int Id_Solicitud { get; set; }
            public string Codigo { get; set; } = string.Empty;
            public string Descripcion { get; set; } = string.Empty;
            public string Cedula { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
            public string Ejecutivo_Id { get; set; } = string.Empty;
            public string Ejecutivo_Nombre { get; set; } = string.Empty;
            public decimal Monto_Base { get; set; }
            public decimal Porc_Venta { get; set; }
            public decimal Comision_Venta { get; set; }
            public decimal Porc_Formaliza { get; set; }
            public decimal Comision_Formaliza { get; set; }
            public string Ejecutivo_Form_Id { get; set; } = string.Empty;
            public string Ejecutivo_Form_Nombre { get; set; } = string.Empty;

            public decimal TotalComision =>
                Comision_Venta + Comision_Formaliza;
        }

        public class CrdComisionesPagoCargaRequest
        {
            public int CodRemesa { get; set; } = 0;
            public List<int> Solicitudes { get; set; } = new List<int>();
            public string Usuario { get; set; } = string.Empty;
        }

        public class CrdComisionesPagoProcesoResponse
        {
            public int CantidadProcesada { get; set; }
            public decimal MontoProcesado { get; set; }
            public string Mensaje { get; set; } = string.Empty;
        }

        public class CrdComisionesPagoCerrarRequest
        {
            public int CodRemesa { get; set; } = 0;
            public string Usuario { get; set; } = string.Empty;
        }
        //Modelos para traslado

        public class CrdComisionesPagoTrasladoModel
        {
            public string Ejecutivo_Id { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
            public string Banco_Desc { get; set; } = string.Empty;
            public decimal Monto { get; set; }
            public int Banco_Id { get; set; }
            public string Banco_Tipo { get; set; } = string.Empty;
            public string Cta_Bancaria { get; set; } = string.Empty;
            public string Identificacion { get; set; } = string.Empty;
            public string Banco_Conta { get; set; } = string.Empty;
        }

        public class CrdComisionesPagoTrasladarRequest
        {
            public int CodRemesa { get; set; } = 0;
            public List<string> Ejecutivos { get; set; } = [];
            public string Usuario { get; set; } = string.Empty;
            public string Aplicacion { get; set; } = "ProGrX";
        }

        //Modelos para reportes
        public class CrdComisionesPagoReporteRequest
        {
            public int CodRemesa { get; set; } = 0;
            public bool Detallado { get; set; }
            public bool Agrupado { get; set; }
            public string Usuario { get; set; } = string.Empty;
        }

        public class CrdComisionesPagoReporteModel
        {
            public int CodRemesa { get; set; } = 0;
            public string Usuario { get; set; } = string.Empty;
            public string Estado { get; set; } = string.Empty;
            public DateTime FechaInicio { get; set; }
            public DateTime FechaCorte { get; set; }
            public string Notas { get; set; } = string.Empty;
            public DateTime Fecha { get; set; }
        }

    }
}
