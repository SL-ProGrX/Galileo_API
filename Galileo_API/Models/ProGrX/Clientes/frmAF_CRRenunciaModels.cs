namespace PgxAPI.Models.ProGrX.Clientes
{
    public class AfRenunciasSocios
    {
        public string Cedula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string CedulaR { get; set; } = string.Empty;
    }

    public class AfRenunciasSocioDetalle
    {
        public string Cedula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public DateTime? FechaIngreso { get; set; }
        public string EstadoActual { get; set; } = string.Empty;
        public int Boleta { get; set; }
        public string EstadoPersona { get; set; } = string.Empty;
        public int Valida { get; set; }
        public int CbrJud { get; set; }
    }

    public class AfRenunciaBancos
    {
        public int Id_Banco { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public string Desc_Corta { get; set; } = string.Empty;
        public string Cta { get; set; } = string.Empty;
        public string Cod_Divisa { get; set; } = string.Empty;
        public int IdX { get; set; }
        public string ItmX { get; set; } = string.Empty;
    }

    public class AfRenunciaBancoFiltro
    {
        public string Usuario { get; set; } = string.Empty;
        public string? Divisa { get; set; }
    }

    public class AfRenunciaEmiteTDocFiltro
    {
        public int BancoId { get; set; }
        public int Mortalidad { get; set; }
        public string Cedula { get; set; } = "A";
        public string TipoRen { get; set; } = "A";
        public int IdCausa { get; set; } = 0;
    }

    public class AfRenunciaEmiteTDoc
    {
        public string IdX { get; set; } = string.Empty;
        public string ItmX { get; set; } = string.Empty;
    }

    public class AfRenunciaCausasDetalle
    {
        public byte Mortalidad { get; set; }
        public byte Liq_Alterna { get; set; }
        public string Tipo_Apl { get; set; } = string.Empty;
        public byte Ajuste_Tasas { get; set; }
    }
    public class AfRenunciaLiqConsultaPatrimonio
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

    public class AfRenunciaExcRentaDetallada
    {
        public decimal Desde { get; set; }
        public decimal Hasta { get; set; }
        public decimal Renta { get; set; }
        public decimal Porcentaje { get; set; }
    }

    public class AfRenunciaLiquidaListaPlanesFiltro
    {
        public string Cedula { get; set; } = string.Empty;
        public string TipoLiq { get; set; } = "A";
    }

    public class AfRenunciaLiquidaListaPlanes
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

    public class AfRenunciaCuentaBancaria
    {
        public string Cuenta_Bancaria { get; set; } = string.Empty;
        public string Cuenta_Desc { get; set; } = string.Empty;
        public string IdX { get; set; } = string.Empty;
        public string ItmX { get; set; } = string.Empty;
        public int Prioridad { get; set; }
    }

    public class AfRenunciaCuentaBancariaFiltro
    {
        public string Identificacion { get; set; } = string.Empty;
        public int BancoId { get; set; }
        public short DivisaCheck { get; set; } = 0;
    }

    public class AfRenunciaPromotor
    {
        public int Id_Promotor { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }

    public class AfRenuncia
    {
        public long Cod_Renuncia { get; set; }
        public string Cedula { get; set; }
        public string Nombre { get; set; }
        public string Estado { get; set; }              // "T","R","P","V"
        public string EstadoActual { get; set; }        // "S","A","P","N"
        public string Tipo { get; set; }                // "A" (Asociación) o "P" (Patronal)
        public DateTime? Vencimiento { get; set; }
        public int? Id_Causa { get; set; }
        public string Causa_Desc { get; set; }
        public int? Id_Promotor { get; set; }
        public string Ejecutivo_Desc { get; set; }
        public bool? Aplica_Reingreso { get; set; }
        public bool? Mortalidad { get; set; }
        public bool? Volver { get; set; }
        public bool? Aumenta_Puntos { get; set; }
        public string Notas { get; set; }
        public int? Id_Banco { get; set; }
        public string? Registro_User {get; set;}
        public string? Resuelto_User { get; set; }
        public DateTime Registro_Fecha { get; set; }
        public DateTime resuelto_Fecha { get; set; }
        public string Banco_Desc { get; set; }
        public string Tipo_Documento { get; set; }
        public string Tipo_Documento_Desc { get; set; }
        public string Cuenta { get; set; }
        public string Cuenta_Desc { get; set; }
        public string Boleta { get; set; }
        public DateTime? Ac_Fecha { get; set; }

    }
    public class AfRenunciaRentaGlobal
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

    public class AfRenunciaRentaGlobalFiltro
    {
        public string Cedula { get; set; } = string.Empty;
        public DateTime? Corte { get; set; }
        public decimal MntRetiro { get; set; }
        public string? Plan { get; set; }
    }

    public class AfRenunciaLiquidacionCreditosPersona
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

    public class AfRenunciaLiquidacionCreditosPersonaFiltro
    {
        public string Cedula { get; set; } = string.Empty;
        public decimal Abono { get; set; } = 0;
    }

    public class AfRenunciaSinpeNegativo
    {
        public decimal Monto_Negativo { get; set; }
        public string Cedula { get; set; } = string.Empty;
    }

    public class AfRenunciaDetalleHistorico
    {
        public long Cod_Renuncia { get; set; }
        public string Cedula { get; set; } = string.Empty;
        public int Id_Causa { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public DateTime? Registro_Fecha { get; set; }
        public string CausaX { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public int Id_Promotor { get; set; }
        public string PromotorX { get; set; } = string.Empty;
    }
    public class AfRenunciaLiquidacion
    {
        public int CodRenuncia { get; set; } 
        public string Cedula { get; set; } = ""; 
        public int IdCausa { get; set; }
        public int IdPromotor { get; set; }
        public bool Mortalidad { get; set; }
        public bool Reingreso { get; set; } 
        public bool AltPlanilla { get; set; }
        public bool Volver { get; set; } 
        public bool AumentoPuntos { get; set; } 
        public bool AporteObrero { get; set; } 
        public bool AportePatronal { get; set; }
        public bool Capitalizacion { get; set; } 
        public bool AhorroExtraordinario { get; set; }
        public bool AceptaPatronal { get; set; }
        public string Tipo { get; set; } = "";
        public string Usuario { get; set; } = ""; 
        public string Notas { get; set; } = ""; 
        public string Oficina { get; set; } = "";
        public string Documento { get; set; } = "";
        public int Banco { get; set; }
        public string Cuenta { get; set; } = "9999999999999999";
        public string CodPlan { get; set; } = "";
        public decimal TotalNeto { get; set; }
        public decimal Disponible { get; set; }
        public decimal RetenerMonto { get; set; }
        public DateTime? AcFecha { get; set; }
        public string Boleta { get; set; } = "";
        public string Equipo { get; set; } = "";
        public string Version { get; set; } = "";
        public int IdDocumento { get; set; }
    }

    public class AfRenunciaPlan
    {
        public int CodRenuncia { get; set; }
        public int CodContrato { get; set; }
        public int CodOperadora { get; set; }
        public string CodPlan { get; set; } = "";
        public decimal Disponible { get; set; }
        public decimal Multa { get; set; }
        public decimal RendPendiente { get; set; }
        public decimal Aportes { get; set; }
        public decimal Rendimientos { get; set; }
        public string CodDivisa { get; set; } = "";
        public decimal TipoCambio { get; set; }
        public bool Marcada { get; set; }
    }

    public class AfRenunciaAbono
    {
        public int CodRenuncia { get; set; }
        public int IdSolicitud { get; set; }
        public string Codigo { get; set; } = "";
        public decimal Abono { get; set; }
        public decimal Saldo { get; set; }
        public decimal Cargos { get; set; }
        public decimal MoraIntC { get; set; }
        public decimal MoraIntM { get; set; }
        public decimal MoraPrin { get; set; }
        public string CodDivisa { get; set; } = "";
        public decimal TipoCambio { get; set; }
        public string Tipo { get; set; } = "";
        public string Garantia { get; set; } = "";
    }
}
