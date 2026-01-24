namespace Galileo_API.Models.ProGrX_Nucleo
{
    public class SugefTipoCambioResult
    {
        public decimal? TC { get; set; }
    }

    public class SugefRomMonitorConsultaResult
    {
        public DateTime Corte { get; set; }
        public string? NumerdoIdentificacion { get; set; }
        public string? PrimerApellidoCliente { get; set; }
        public string? SegundoApellidoCliente { get; set; }
        public string? NombreCliente { get; set; }
        public decimal? MontoMovimiento { get; set; }
        public decimal? SalarioPromedio { get; set; }
        public decimal? Monto_Col { get; set; }
        public decimal? Salario_Col { get; set; }
        public decimal? Monto_Dol { get; set; }
        public decimal? Salario_Dol { get; set; }
        public string? NombreClienteCompleto { get; set; }
        public string? Cedula { get; set; }
        public string? Tipo_Id_Desc { get; set; }
    }

    public class SugefRomMonitorDetalleResult
    {
        public string? TipoDoc { get; set; }
        public string? NumDoc { get; set; }
        public decimal? Monto_Col { get; set; }
        public decimal? Monto_Dol { get; set; }
        public string? Tipo_Doc_Desc { get; set; }
        public string? Concepto_Desc { get; set; }
        public DateTime? REGISTRO_FECHA { get; set; }
        public string? REGISTRO_USUARIO { get; set; }
    }

    public class SugefRomMonitorFormaPagoResult
    {
        public DateTime Corte { get; set; }
        public string? COD_LINEA { get; set; }
        public string? TIPO_DOCUMENTO { get; set; }
        public string? COD_TRANSACCION { get; set; }
        public string? COD_FORMA_PAGO { get; set; }
        public decimal? Monto_Col { get; set; }
        public decimal? Monto_Dol { get; set; }
        public string? Forma_Pago_Desc { get; set; }
        public string? Concepto_Desc { get; set; }
        public string? Origen_Recursos_Desc { get; set; }
        public string? Pagador_Desc { get; set; }
        public DateTime? REGISTRA_FECHA { get; set; }
        public string? REGISTRA_USUARIO { get; set; }
    }

    public class SugefRomMonitorParams
    {
        public DateTime Corte { get; set; }
        public decimal BaseDol { get; set; }
        public string Usuario { get; set; } = string.Empty;
    }

    public class SugefRomMonitorFormaPagoActualizaParams
    {
        public int LineaId { get; set; }
        public string TipoDoc { get; set; } = string.Empty;
        public string NumDoc { get; set; } = string.Empty;
        public string PagadorId { get; set; } = string.Empty;
        public string Origen { get; set; } = string.Empty;
        public string Notas { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
    }
}
