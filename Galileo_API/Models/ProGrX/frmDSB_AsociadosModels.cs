namespace Galileo.Models.ProGrX
{
    public class DashboardAsociadosPuntoData
    {
        public string? Descripcion { get; set; }
        public decimal? Value { get; set; }
    }

    public class DashboardAsociadosData
    {
        public string CedulaX { get; set; } = string.Empty;
        public string? Nombre { get; set; }
        public DateTime? FechaIngreso { get; set; }
        public string? EstadoActual { get; set; }
        public string? Notas { get; set; }
        public bool? Bloqueo { get; set; }
        public string? Nota_User { get; set; }
        public DateTime? Nota_Fecha { get; set; }
        public decimal? Obrero { get; set; }
        public decimal? Patronal { get; set; }
        public decimal? Custodia { get; set; }
        public decimal? Capitaliza { get; set; }
        public string? Cod_Divisa { get; set; }
        public decimal? Ahorro { get; set; }
        public decimal? Aporte { get; set; }
        public decimal? Extra { get; set; }
        public DateTime? Fecextra { get; set; }
        public DateTime? Fecahorro { get; set; }
        public DateTime? Fecaporte { get; set; }
        public DateTime? Feccustodia { get; set; }
        public DateTime? Feccapitaliza { get; set; }
        public string? Clasificacion { get; set; }
        public int? IndMensajes { get; set; }
        public int? IndCobro { get; set; }
        public int? IndFianzas { get; set; }
        public int? IndAdvertencias { get; set; }
        public int? IndBeneficiarios { get; set; }
        public string? InstitucionX { get; set; }
        public string? EstadoX { get; set; }
        public string? Deductora { get; set; }
        public DateTime? Ben_Update_Fecha { get; set; }
        public string? Ben_Update_Usuario { get; set; }
        public DateTime? Consentimiento_Contacto_Fecha { get; set; }
        public string? Consentimiento_Contacto_Usuario { get; set; }
        public int? Edad { get; set; }
        public string? Membresia { get; set; }
        public decimal? Creditos_Monto { get; set; }
        public decimal? Creditos_Saldo { get; set; }
        public decimal? Creditos_Cuota { get; set; }
        public DateTime? Creditos_Ultimo { get; set; }
        public int? Creditos_Operaciones { get; set; }
        public decimal? Retencion_Saldo { get; set; }
        public decimal? Retencion_Cuota { get; set; }
        public DateTime? Retencion_Ultimo { get; set; }
        public int? Retencion_Operaciones { get; set; }
        public string? Mora_Antiguedad { get; set; }
        public decimal? Fondos_Acumulado { get; set; }
        public decimal? Fondos_Mensualidad { get; set; }
        public int? Fondos_Contratos { get; set; }
        public DateTime? Fondos_Ultimo { get; set; }
        public string? SC_Endeudamiento { get; set; }
        public string? SC_Historial_Pago { get; set; }
        public string? SC_Morosidad { get; set; }
        public string? SC_Capacidad { get; set; }
        public string? SC_Garantia { get; set; }
        public string? SC_Capacidad_Color { get; set; }
        public string? SC_Endeudamiento_Color { get; set; }
        public string? SC_Garantia_Color { get; set; }
        public string? SC_Historial_Color { get; set; }
        public string? SC_Morosidad_Color { get; set; }
        public DateTime? SC_Fecha { get; set; }
        public decimal? SC_Salario_Devengado { get; set; }
        public decimal? SC_Salario_Liquido { get; set; }
        public decimal? SC_Liquidez_Simple { get; set; }
        public decimal? SC_Liquidez_WFinanzas { get; set; }
        public DateTime? Fecha_Liquidación { get; set; }
        public string? ReIngreso { get; set; }
        public string? Generacion_Desc { get; set; }
        public decimal? Disponible_SobreAhorros { get; set; }
        public decimal? Disponible_Excedentes { get; set; }
        public DateTime? Ultimo_Beneficio { get; set; }
        public DateTime? FechaIngresoReferencia { get; set; }
        public string MembresiaCaption { get; set; } = string.Empty;
        public string MembresiaToolTip { get; set; } = string.Empty;
        public bool MembresiaEsRenuncia { get; set; }
        public string EstadoBeneficiariosToolTip { get; set; } = string.Empty;
        public string EstadoConsentimientoToolTip { get; set; } = string.Empty;
        public List<DashboardAsociadosPuntoData> Patrimonio { get; set; } = [];
        public List<DashboardAsociadosPuntoData> Creditos { get; set; } = [];
        public List<DashboardAsociadosPuntoData> Fondos { get; set; } = [];
        public List<DashboardAsociadosPuntoData> Beneficios { get; set; } = [];
    }

    public class DashboardAsociadosRenunciaData
    {
        public string? Cod_Renuncia { get; set; }
        public DateTime? Registro_fecha { get; set; }
        public string? registro_user { get; set; }
        public string? Estado { get; set; }
        public string? Tipo { get; set; }
        public string? Descripcion { get; set; }
    }
}
