namespace Galileo.Models.ProGrX.Clientes
{
    public class FrmAfLiquidacionWModels
    {
        public class AfLiquidacionBancos
        {
            public int Id_Banco { get; set; }
            public string Descripcion { get; set; } = string.Empty;
            public string Desc_Corta { get; set; } = string.Empty;
            public string Cta { get; set; } = string.Empty;
            public string Cod_Divisa { get; set; } = string.Empty;
            public int IdX { get; set; }
            public string ItmX { get; set; } = string.Empty;
        }

        public class AfLiquidacionBancosFiltro
        {
            public string Usuario { get; set; } = string.Empty;
            public string? Divisa { get; set; }
        }

        public class AfLiquidacionEmiteTDocFiltro
        {
            public int BancoId { get; set; }
            public int Mortalidad { get; set; }
            public string Cedula { get; set; } = "A";
            public string TipoRen { get; set; } = "A";
            public int IdCausa { get; set; } = 0;
        }

        public class AfLiquidacionEmiteTDoc
        {
            public string IdX { get; set; } = string.Empty;
            public string ItmX { get; set; } = string.Empty;
        }

        public class AfLiquidacionCausasDetalle
        {
            public byte Mortalidad { get; set; }
            public byte Liq_Alterna { get; set; }
            public string Tipo_Apl { get; set; } = string.Empty;
            public byte Ajuste_Tasas { get; set; }
        }

        public class AfLiquidacionCuentaBancaria
        {
            public string Cuenta_Bancaria { get; set; } = string.Empty;
            public string Cuenta_Desc { get; set; } = string.Empty;
            public string IdX { get; set; } = string.Empty;
            public string ItmX { get; set; } = string.Empty;
            public int Prioridad { get; set; }
        }

        public class AfLiquidacionCuentaBancariaFiltro
        {
            public string Identificacion { get; set; } = string.Empty;
            public int BancoId { get; set; }
            public short DivisaCheck { get; set; } = 0;
        }

    }   

    public class AfLiquidacionRenunciaSinLiquidar
    {
        public string Cedula { get; set; } = string.Empty;
        public string Id_Alterno { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
    }

    public class AfLiquidacionSocio
    {
        public string Cedula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
    }

    public class AfLiquidacionSocioDetalle
    {
        public string Cedula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public DateTime FechaIngreso { get; set; }
        public string EstadoActual { get; set; } = string.Empty;
        public int Boleta { get; set; }
        public string EstadoPersona { get; set; } = string.Empty;
    }

    public class AfLiquidacionCausaAccion
    {
        public byte Mortalidad { get; set; }
        public byte Liq_Alterna { get; set; }
    }

    public class AfLiquidacionSocioExiste
    {
        public int Existe { get; set; }
    }

    public class AfLiquidacionConsultaPatrimonio
    {
        public decimal Ahorro { get; set; }
        public decimal Aporte { get; set; }
        public decimal Capitaliza { get; set; }
        public decimal Extra { get; set; }
        public decimal Custodia { get; set; }
        public decimal Renta { get; set; }
        public decimal Excedente { get; set; }
        public decimal Exc_Renta { get; set; }
        public short Exc_Aplica { get; set; }
        public string Cod_Divisa { get; set; } = string.Empty;
        public decimal Tipo_Cambio { get; set; }
        public string Divisa_Local { get; set; } = string.Empty;
    }

    public class AfLiquidacionRentaGlobal
    {
        public string Cedula { get; set; } = string.Empty;
        public decimal RG_Porcentaje { get; set; }
        public decimal RG_MntNoGravable { get; set; }
        public decimal Retiro_Acumulado { get; set; }
        public decimal Retiro_Monto { get; set; }
        public decimal Retiro_Gravable { get; set; }
        public decimal ISR_Monto { get; set; }
        public short RG_Aplica { get; set; }
    }

    public class AfLiquidacionRentaGlobalFiltro
    {
        public string Cedula { get; set; } = string.Empty;
        public DateTime? Corte { get; set; }
        public decimal MntRetiro { get; set; }
        public string? Plan { get; set; }
    }

    public class AfLiquidacionListaPlanesFiltro
    {
        public string Cedula { get; set; } = string.Empty;
        public string TipoLiq { get; set; } = "A";
    }

    public class AfLiquidacionListaPlanes
    {
        public int Cod_Contrato { get; set; }
        public string Cod_Plan { get; set; } = string.Empty;
        public int Cod_Operadora { get; set; }
        public decimal Aportes { get; set; }
        public decimal Rendimiento { get; set; }
        public string PlanX { get; set; } = string.Empty;
        public string OperadoraX { get; set; } = string.Empty;
        public decimal RendPendiente { get; set; }
        public decimal Renta_Global { get; set; }
        public decimal Impuesto_Renta { get; set; }
        public decimal Multa { get; set; }
        public string Cod_Divisa { get; set; } = string.Empty;
        public decimal Tipo_Cambio { get; set; }
    }

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

    public class AfRenunciaSifModel
    {
        public int IdCausa { get; set; }
        public string Cedula { get; set; } = string.Empty;
        public DateTime FechaSistema { get; set; }
        public string TipoRenFlag { get; set; } = string.Empty; // 'A' o 'P'
    }

    public class AfRenunciaAseModel
    {
        public int IdCausa { get; set; }
        public int IdPromotor { get; set; }
        public string Cedula { get; set; } = string.Empty;
        public int IdBoletaAf { get; set; }
        public DateTime FechaSistema { get; set; }
        public string TipoRenFlag { get; set; } = string.Empty; // 'A' o 'P'
        public int? Nacta { get; set; }
        public byte Mortalidad { get; set; }
    }

    public class AfSocioDatosBasicos
    {
        public string Cedula { get; set; } = string.Empty;
        public int Nacta { get; set; }
        public int Id_Promotor { get; set; }
        public int Id_Boleta_Af { get; set; }
    }

    public class AfLiquidacionInsertModel
    {
        public string Cedula { get; set; } = string.Empty;
        public decimal Ahorro { get; set; }
        public decimal Aporte { get; set; }
        public decimal Custodia { get; set; }
        public decimal Capitaliza { get; set; }
        public decimal Extra { get; set; }
        public decimal FCI { get; set; }
        public decimal Retenido { get; set; }
        public DateTime FechaIngreso { get; set; }
        public string TipoRenFlag { get; set; } = string.Empty;
        public string EstadoActual { get; set; } = string.Empty;
        public byte AplAhorro { get; set; }
        public byte AplAporte { get; set; }
        public byte AplCapitalizado { get; set; }
        public byte AplExtra { get; set; }
        public decimal TotalBrutoUI { get; set; }
        public decimal MontoAGirar { get; set; }
        public decimal Ahorro_Liq { get; set; }
        public decimal Aporte_Liq { get; set; }
        public decimal Custodia_Liq { get; set; }
        public decimal Capitaliza_Liq { get; set; }
        public decimal Extra_Liq { get; set; }
        public string TipoDoc { get; set; } = string.Empty;
        public int CodBanco { get; set; }
        public string CuentaAhorros { get; set; } = string.Empty;
        public byte Mortalidad { get; set; }
        public int IdCausa { get; set; }
        public string Observacion { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public int CodOficina { get; set; }
        public int? AcBoleta { get; set; }
        public DateTime? AcFecha { get; set; }
        public DateTime? FechaPago { get; set; }
        public decimal Exc_Periodo { get; set; }
        public decimal Exc_Ir { get; set; }
        public decimal Exc_Liq { get; set; }
        public decimal Exc_Ir_Liq { get; set; }
        public byte Apl_Excedente { get; set; }
        public string Cod_Divisa { get; set; } = string.Empty;
        public decimal Tipo_Cambio { get; set; }
    }

    public class AfLiquidacionPatrimonioInput
    {
        public int LiqConsec { get; set; }
        public string Usuario { get; set; } = string.Empty;
    }

    public class AfLiquidaFondosInsertModel
    {
        public int LiqConsec { get; set; }
        public int CodContrato { get; set; }
        public int CodOperadora { get; set; }
        public string CodPlan { get; set; } = string.Empty;
        public decimal Disponible { get; set; }
        public decimal Multa { get; set; }
        public decimal RendPendiente { get; set; }
        public decimal Aportes { get; set; }
        public decimal Rendimientos { get; set; }
        public string CodDivisa { get; set; } = string.Empty;
        public decimal TipoCambio { get; set; }
    }

    public class AfLiquidaPlanesInput
    {
        public int LiqConsec { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public string OficinaTitular { get; set; } = string.Empty;
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
