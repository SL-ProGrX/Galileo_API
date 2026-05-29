namespace Galileo.Models.ProGrX.Clientes
{
    public class AfRenunciaSifModel
    {
        public int IdCausa { get; set; }
        public string Cedula { get; set; } = string.Empty;
        public DateTime FechaSistema { get; set; }
        public string TipoRenFlag { get; set; } = string.Empty;
    }

    public class AfRenunciaAseModel
    {
        public int IdCausa { get; set; }
        public int IdPromotor { get; set; }
        public string Cedula { get; set; } = string.Empty;
        public int IdBoletaAf { get; set; }
        public DateTime FechaSistema { get; set; }
        public string TipoRenFlag { get; set; } = string.Empty;
        public int? Nacta { get; set; }
        public byte Mortalidad { get; set; }
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
}