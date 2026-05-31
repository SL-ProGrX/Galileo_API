namespace Galileo.Models.ProGrX.Clientes
{
    public class AfLiquidacionCreditosPersona
    {
        public int Id_Prioridad { get; set; }
        public int Id_Solicitud { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal Saldo { get; set; }
        public string GarantiaX { get; set; } = string.Empty;
        public decimal INTC { get; set; }
        public decimal INTM { get; set; }
        public decimal Amortiza { get; set; }
        public decimal Cargos { get; set; }
        public decimal Polizas { get; set; }
        public string Prioridad { get; set; } = string.Empty;
        public string Detalle { get; set; } = string.Empty;
        public string Cod_Divisa { get; set; } = string.Empty;
        public decimal Tipo_Cambio { get; set; }
        public decimal IVA { get; set; }
        public decimal Mora { get; set; }
        public double TC_APL { get; set; }
        public decimal Abono { get; set; }
        public short Sobre_Ahorros { get; set; }
    }

    public class AfLiquidacionCreditosPersonaFiltro
    {
        public string Cedula { get; set; } = string.Empty;
        public decimal Abono { get; set; } = 0;
    }

    public class AfLiquidacionCodRenuncia
    {
        public int Cod_Renuncia { get; set; }
    }

    public class AfLiquidaDetalleInsertModel
    {
        public int LiqConsec { get; set; }
        public int IdSolicitud { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public decimal AbonoFila { get; set; }
        public decimal SaldoFila { get; set; }
        public string CodDivisa { get; set; } = string.Empty;
        public decimal TipoCambio { get; set; }
    }

    public class AfMorosidadModel
    {
        public int LiqConsec { get; set; }
        public int IdSolicitud { get; set; }
        public string Usuario { get; set; } = string.Empty;
    }

    public class AfMorosidadPorMoraModel
    {
        public int id_moro { get; set; }
        public decimal AbIntC { get; set; }
        public decimal AbIntM { get; set; }
        public decimal AbAmortiza { get; set; }
        public int LiqConsec { get; set; }
        public string Usuario { get; set; } = string.Empty;
    }

    public class AfMorosidadInsertModel
    {
        public int IdSolicitud { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public decimal IntC { get; set; }
        public decimal IntM { get; set; }
        public decimal Amortiza { get; set; }
        public decimal AbIntC { get; set; }
        public decimal AbIntM { get; set; }
        public decimal AbAmortiza { get; set; }
        public int LiqConsec { get; set; }
        public DateTime Fechap { get; set; }
        public DateTime Fecap { get; set; }
        public string Usuario { get; set; } = string.Empty;
    }

    public class AfMorosidadConsultaModel
    {
        public int id_moro { get; set; }
        public int id_solicitud { get; set; }
        public string codigo { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string estadoi { get; set; } = string.Empty;
        public decimal intc { get; set; }
        public decimal intm { get; set; }
        public decimal amortiza { get; set; }
        public decimal abintc { get; set; }
        public decimal abintm { get; set; }
        public decimal abamortiza { get; set; }
        public string tcon { get; set; } = string.Empty;
        public int ncon { get; set; }
        public DateTime fechap { get; set; }
        public DateTime fecap { get; set; }
        public DateTime fecult { get; set; }
        public decimal cuota_morosa { get; set; }
        public string usuario { get; set; } = string.Empty;
        public string cod_concepto { get; set; } = string.Empty;
        public string cod_caja { get; set; } = string.Empty;
    }

    public class AfRegCreditosActualizarModel
    {
        public int IdSolicitud { get; set; }
        public decimal CurPrin { get; set; }
        public decimal CurIntC { get; set; }
        public decimal CurIntM { get; set; }
    }

    public class AfCreditosDtInsertModel
    {
        public int IdSolicitud { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public decimal curAbono { get; set; }
        public DateTime FechaCR { get; set; }
        public int LiqConsec { get; set; }
        public string Usuario { get; set; } = string.Empty;
    }
}