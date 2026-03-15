namespace Galileo_API.Models.ProGrX_Contabilidad
{
    public class CntXPlantillaAsientosDto
    {
        public string Cod_Plantilla { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public int Consecutivo { get; set; }
        public string Anio_Inicio { get; set; } = string.Empty;
        public string Mes_Inicio { get; set; } = string.Empty;
        public string Tipo_Asiento { get; set; } = string.Empty;
        public string Asiento_Descripcion { get; set; } = string.Empty;
        public string Asiento_Documento { get; set; } = string.Empty;
        public string Asiento_Detalle { get; set; } = string.Empty;
    }

    public class CntXPlantillaAsientosUpdateParams
    {
        public required int CodContabilidad { get; set; }
        public string CodPlantilla { get; set; } = string.Empty;
        public required int Consecutivo { get; set; }
    }

    public class CntxAsientosInsertParams
    {
        public required int CodContabilidad { get; set; }
        public string TipoAsiento { get; set; } = string.Empty;
        public string NumAsiento { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string FechaAsiento { get; set; } = string.Empty;
        public int? Anio { get; set; }
        public int? Mes { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public string Notas { get; set; } = string.Empty;
    }

    public class CntXPlantillaDetalleDto
    {
        public string Cod_Plantilla { get; set; } = string.Empty;
        public int Num_Linea { get; set; }
        public string Cod_Cuenta { get; set; } = string.Empty;
        public decimal Monto_Debito { get; set; }
        public decimal Monto_Credito { get; set; }
        public string Documento { get; set; } = string.Empty;
        public string Detalle { get; set; } = string.Empty;
        public string Cod_Unidad { get; set; } = string.Empty;
        public string Cod_Divisa { get; set; } = string.Empty;
        public decimal Tipo_Cambio { get; set; }
        public string Cod_Centro_Costo { get; set; } = string.Empty;
    }

    public class CntxAsientosDetalleInsertParams
    {
        public required int CodContabilidad { get; set; }
        public string TipoAsiento { get; set; } = string.Empty;
        public string NumAsiento { get; set; } = string.Empty;
        public string CodCuenta { get; set; } = string.Empty;
        public decimal? MontoDebito { get; set; }
        public decimal? MontoCredito { get; set; }
        public string Documento { get; set; } = string.Empty;
        public string Detalle { get; set; } = string.Empty;
        public int? NumLinea { get; set; }
        public string CodUnidad { get; set; } = string.Empty;
        public string CodDivisa { get; set; } = string.Empty;
        public decimal? TipoCambio { get; set; }
        public string CodCentroCosto { get; set; } = string.Empty;
    }
}
