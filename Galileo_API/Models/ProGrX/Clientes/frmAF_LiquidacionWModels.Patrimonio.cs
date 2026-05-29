namespace Galileo.Models.ProGrX.Clientes
{
    public class AfLiquidacionCausaAccion
    {
        public byte Mortalidad { get; set; }
        public byte Liq_Alterna { get; set; }
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
}