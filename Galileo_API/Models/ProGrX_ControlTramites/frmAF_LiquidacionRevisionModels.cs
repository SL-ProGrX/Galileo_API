namespace Galileo_API.Models.ProGrX_ControlTramites
{
    public class AfLiquidacionRevisionListaModel
    {
        public string Tipo { get; set; } = string.Empty;
        public string Cedula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Usuario_Registra { get; set; } = string.Empty;
        public string No_Remesa { get; set; } = string.Empty;
        public string Usuario_Remesa { get; set; } = string.Empty;
        public string No_Boleta { get; set; } = string.Empty;
    }

    public class AfLiquidacionRevisionDetalleModel
    {
        public int Consec { get; set; }
        public string Cedula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public decimal Ahorro_Liq { get; set; }
        public decimal Aporte_Liq { get; set; }
        public decimal Capitalizado_Liq { get; set; }
        public decimal Total_Bruto { get; set; }
        public decimal T_Neto { get; set; }
        public decimal Retenido { get; set; }
        public string Ac_Boleta { get; set; } = string.Empty;
        public DateTime? Ac_Fecha { get; set; }
        public DateTime? Fecliq { get; set; }
        public string Tdocumento { get; set; } = string.Empty;
        public string Banco { get; set; } = string.Empty;
        public string Causa { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string Cuenta { get; set; } = string.Empty;
    }

    public class AfLiquidacionRevisionOperacionModel
    {
        public string Id_Solicitud { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
        public decimal Abono { get; set; }
        public decimal Saldo { get; set; }
        public decimal Resultante { get; set; }
    }

    public class AfLiquidacionRevisionSeguimientoModel
    {
        public string Descripcion { get; set; } = string.Empty;
        public string Notas { get; set; } = string.Empty;
        public DateTime? Registro_Fecha { get; set; }
        public string Registro_Usuario { get; set; } = string.Empty;
    }

    public class AfLiquidacionRevisionEtiquetaModel
    {
        public string Tag_Codigo { get; set; } = string.Empty;
        public string ItmX { get; set; } = string.Empty;
    }

    public class AfLiquidacionRevisionOmisionModel
    {
        public string Id_Error { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Asignado { get; set; } = string.Empty;
        public string Aplicado { get; set; } = "N";
        public string Mensaje { get; set; } = string.Empty;
        public string Linea_Err { get; set; } = string.Empty;
    }

    public class AfLiquidacionRevisionAvisoModel
    {
        public string Mensaje { get; set; } = string.Empty;
    }

    public class AfLiquidacionRevisionOmisionInsertarRequest
    {
        public string Cedula { get; set; } = string.Empty;
        public string Id_Error { get; set; } = string.Empty;
        public string Documento { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
    }

    public class AfLiquidacionRevisionOmisionInsertarModel
    {
        public string Linea_Err { get; set; } = string.Empty;
    }

    public class AfLiquidacionRevisionOmisionEliminarRequest
    {
        public string Linea_Err { get; set; } = string.Empty;
    }

    public class AfLiquidacionRevisionOmisionesAplicarRequest
    {
        public string Cedula { get; set; } = string.Empty;
        public string Documento { get; set; } = string.Empty;
    }

    public class AfLiquidacionRevisionAplicarRequest
    {
        public string Cedula { get; set; } = string.Empty;
        public string Documento { get; set; } = string.Empty;
        public string Tag { get; set; } = string.Empty;
        public string Observacion { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
    }
}
