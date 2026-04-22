namespace Galileo.Models.ProGrX.Cajas
{
    public class FiltrosMovimientosFormaPago : FiltrosLazyLoadData
    {
        public string Usuario { get; set; } = "";
        public bool TodasLasFechas { get; set; } = false;
        public string FechaInicio { get; set; } = "";
        public string FechaCorte { get; set; } = "";
        public string Cedula { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string NumDoc { get; set; } = "";
        public string CodFormaPago { get; set; } = "";
        public bool MostrarSaldosFavorRelacionados { get; set; } = false;
        public string CodCaja { get; set; } = "";
        public long? CodApertura { get; set; } = null;
        public string TipoMov { get; set; } = "T";
        public string CodEntidadPago { get; set; } = "";
        public string CodOrigenRecursos { get; set; } = "";
    }

    public class CajasMovimientosFormaPagoLista
    {
        public int Total { get; set; } = 0;
        public List<CajasMovimientoFormaPagoItem> Lista { get; set; } = new();
        public decimal TotalMontoAplicado { get; set; } = 0;
    }

    public class CajasMovimientoFormaPagoItem
    {
        public string Cliente_Identificacion { get; set; } = "";
        public string Cliente_Nombre { get; set; } = "";
        public string TipoDocDesc { get; set; } = "";
        public string Cod_Transaccion { get; set; } = "";
        public decimal? Monto_Doc { get; set; }
        public decimal? Monto_Aplicado { get; set; }
        public string Cod_Divisa { get; set; } = "";
        public decimal? Tipo_Cambio { get; set; }
        public string Registro_Fecha_Format { get; set; } = "";
        public string Registro_Usuario { get; set; } = "";
        public string FormaPagoDesc { get; set; } = "";
        public string Num_Referencia { get; set; } = "";
        public string BancoDesc { get; set; } = "";
        public string OrigenRecursoDesc { get; set; } = "";
        public string EntidadPagoDesc { get; set; } = "";
        public string Cod_Cuenta { get; set; } = "";
        public string ConceptoDesc { get; set; } = "";
        public string Cod_Caja { get; set; } = "";
        public long? Cod_Apertura { get; set; }
    }

    public class CajasUltimaAperturaDto
    {
        public long Resultado { get; set; }
    }

}
