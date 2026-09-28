namespace Galileo_API.Models.ProGrX.CuentasxCobrar
{
    public class CxcConceptoDto
    {
        public string Cod_Concepto { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string? Cod_Cuenta { get; set; }
        public string? Cod_Cuenta_Salida { get; set; }
        /// <summary>Divisa derivada de CntX_Cuentas (solo lectura; no se persiste en CxC_Conceptos).</summary>
        public string? Cod_Divisa { get; set; }
        public short Requiere_Contrato { get; set; }
        public short Requiere_Documento { get; set; }
        public short Genera_Desembolso { get; set; }
        public short Proceso_Descuento { get; set; }
        public short Activo { get; set; }
        public short Adelanto_Informativo { get; set; }
        public DateTime? Registro_Fecha { get; set; }
        public string? Registro_Usuario { get; set; }
        public string? Pagador_Default { get; set; }
        public decimal? Monto_Max { get; set; }
        public string? Cod_Unidad { get; set; }
        public string? Cod_Centro_Costo { get; set; }
        public short I_Indicador { get; set; }
        public string? I_Cta_Deterioro { get; set; }
        public string? I_Cta_Estimacion { get; set; }
        public string? I_Cta_Orden_Debe { get; set; }
        public string? I_Cta_Orden_Haber { get; set; }
        public string? I_Cta_Ingreso { get; set; }
        public string? Modifica_Usuario { get; set; }
        public DateTime? Modifica_Fecha { get; set; }
    }

    public class CxcConceptoExisteResult
    {
        public int Existe { get; set; }
    }

    public class CxcConceptoSaveParams
    {
        public string Cod_Concepto { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string? Cod_Cuenta { get; set; }
        public string? Cod_Cuenta_Salida { get; set; }
        public short? Requiere_Contrato { get; set; }
        public short? Requiere_Documento { get; set; }
        public short? Genera_Desembolso { get; set; }
        public short? Proceso_Descuento { get; set; }
        public decimal? Monto_Max { get; set; }
        public short? Activo { get; set; }
        public string Usuario { get; set; } = string.Empty;
    }

    public class CxcConceptoDeleteParams
    {
        public string Cod_Concepto { get; set; } = string.Empty;
    }

    public class CxcConceptoAsignacionDto
    {
        public string Codigo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public DateTime? Registro_Fecha { get; set; }
        public string? Registro_Usuario { get; set; }
    }

    public class CxcConceptoContratoParams
    {
        public string Cod_Contrato { get; set; } = string.Empty;
        public string Cod_Concepto { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
    }

    public class CxcConceptoFacturaEstadoParams
    {
        public string Factura_Estado { get; set; } = string.Empty;
        public string Cod_Concepto { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
    }

    public class CxcPersonaDto
    {
        public string Cedula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
    }
    public class CxcConceptoPagadorDefaultParams
    {
        public string Cod_Concepto { get; set; } = string.Empty;
        public string Pagador_Default { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
    }

    public class UnidadDto
    {
        public string Unidad { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }

    public class CentrosCostoDto
    {
        public string Centro { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }

    public class CxcConceptoIncobrableParams
    {
        public string Cod_Concepto { get; set; } = string.Empty;
        public short? Indicador { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public string Cod_Unidad { get; set; } = string.Empty;
        public string Cod_Centro_Costo { get; set; } = string.Empty;
        public string Cta_Deterioro { get; set; } = string.Empty;
        public string Cta_Estimacion { get; set; } = string.Empty;
        public string Cta_Ingreso { get; set; } = string.Empty;
        public string Cta_Orden_Debe { get; set; } = string.Empty;
        public string Cta_Orden_Haber { get; set; } = string.Empty;
    }

    /// <summary>
    /// Resultado de spCxC_Concepto_Incobrable_Consulta (campos alineados al VB6).
    /// </summary>
    public class CxcConceptoIncobrableConsultaDto
    {
        public short I_Indicador { get; set; }
        public string? Cod_Unidad { get; set; }
        public string? Unidad_Desc { get; set; }
        public string? Cod_Centro_Costo { get; set; }
        public string? Centro_Desc { get; set; }
        public string? Cta_Deterioro_Mask { get; set; }
        public string? Cta_Deterioro_Desc { get; set; }
        public string? Cta_Estimacion_Mask { get; set; }
        public string? Cta_Estimacion_Desc { get; set; }
        public string? Cta_Ingreso_Mask { get; set; }
        public string? Cta_Ingreso_Desc { get; set; }
        public string? Cta_Orden_Debe_Mask { get; set; }
        public string? Cta_Orden_Debe_Desc { get; set; }
        public string? Cta_Orden_Haber_Mask { get; set; }
        public string? Cta_Orden_Haber_Desc { get; set; }
    }
}
