namespace Galileo_API.Models.ProGrX.General
{
    public class CcAnomaliaFiltroDto
    {
        public decimal? Monto { get; set; }
        public string? Linea { get; set; }
        public string? Destino { get; set; }
        public int? Institucion { get; set; }
    }

    public class CcAnomaliaCtaDerivadaFiltroDto
    {
        public decimal? Monto { get; set; }
    }

    public class CcAnomaliaCreditoItemDto
    {
        public string Codigo { get; set; } = string.Empty;
        public int Id_Solicitud { get; set; }
        public string Cedula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public decimal Saldo { get; set; }
        public string Opex { get; set; } = string.Empty;
        public string Proceso { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string? Estadosol { get; set; }
        public decimal? MoraFinanciera { get; set; }
        public string Institucion { get; set; } = string.Empty;
        public string LineaDesc { get; set; } = string.Empty;
        public string Destino { get; set; } = string.Empty;
    }

    public class CcAnomaliaCtaDerivadaItemDto
    {
        public int Id_Solicitud { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Cedula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public decimal Saldo { get; set; }
        public int Num_Cuota { get; set; }
        public decimal Monto { get; set; }
        public string Descripcion { get; set; } = string.Empty;
    }

    /// <summary>
    /// Cuenta del parámetro CRD_PARAMETROS (VB6 fxCrdParametro) con máscara y descripción.
    /// </summary>
    public class CcAnomaliaCuentaOpcionDto
    {
        public string Cod_Cuenta { get; set; } = string.Empty;
        public string Cuenta_Mask { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request para corregir saldos menores (VB6 sbCorrigeSaldoMenor).
    /// </summary>
    public class CcAnomaliaSaldosMenoresCorregirRequest
    {
        public string Usuario { get; set; } = string.Empty;
        public string Cuenta { get; set; } = string.Empty;
        public string? Linea { get; set; }
        public string? Destino { get; set; }
        public int? Institucion { get; set; }
    }

    /// <summary>
    /// Resultado de la corrección de saldos menores.
    /// </summary>
    public class CcAnomaliaSaldosMenoresCorregirResultado
    {
        public string Mensaje { get; set; } = string.Empty;
        public string Tipo_Documento { get; set; } = string.Empty;
        public long Numero_Documento { get; set; }
        public decimal Total_Corregido { get; set; }
        public int Casos { get; set; }
    }

    /// <summary>
    /// Request para corregir saldos negativos (VB6 sbCorrigeSaldoNegativo).
    /// </summary>
    public class CcAnomaliaSaldosNegativosCorregirRequest
    {
        public string Usuario { get; set; } = string.Empty;
        public string Cuenta { get; set; } = string.Empty;
        public string? Linea { get; set; }
        public string? Destino { get; set; }
        public int? Institucion { get; set; }
    }

    /// <summary>
    /// Resultado de la corrección de saldos negativos.
    /// </summary>
    public class CcAnomaliaSaldosNegativosCorregirResultado
    {
        public string Mensaje { get; set; } = string.Empty;
        public string Tipo_Documento { get; set; } = string.Empty;
        public long Numero_Documento { get; set; }
        public decimal Total_Corregido { get; set; }
        public int Casos { get; set; }
    }

    /// <summary>
    /// Request para corregir mora menor (VB6 sbCorrigeMora).
    /// </summary>
    public class CcAnomaliaMoraMenorCorregirRequest
    {
        public string Usuario { get; set; } = string.Empty;
        public string? Linea { get; set; }
        public string? Destino { get; set; }
        public int? Institucion { get; set; }
    }

    /// <summary>
    /// Resultado de la corrección de mora menor.
    /// </summary>
    public class CcAnomaliaMoraMenorCorregirResultado
    {
        public string Mensaje { get; set; } = string.Empty;
        public decimal Monto_Limite { get; set; }
    }

    /// <summary>
    /// Request para corregir cta. derivada menor (VB6 sbCtaDerivada_Corrige).
    /// </summary>
    public class CcAnomaliaCtaDerivadaCorregirRequest
    {
        public string Usuario { get; set; } = string.Empty;
    }

    /// <summary>
    /// Resultado de spSys_Creditos_Clean_Ctas_Menores / corrección cta. derivada.
    /// </summary>
    public class CcAnomaliaCtaDerivadaCorregirResultado
    {
        public string Mensaje { get; set; } = string.Empty;
        public string Tipo_Documento { get; set; } = string.Empty;
        public string Numero_Documento { get; set; } = string.Empty;
    }

    public class CcAnomaliaOficinaOmisionDto
    {
        public string Cod_Unidad { get; set; } = string.Empty;
        public string Cod_Centro_Costo { get; set; } = string.Empty;
    }

    public class CcAnomaliaOperacionCtasDto
    {
        public int Id_Solicitud { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Cod_Divisa { get; set; } = "COL";
        public string Cod_Unidad { get; set; } = string.Empty;
        public string Cod_Centro_Costo { get; set; } = string.Empty;
        public string CtaAmortiza { get; set; } = string.Empty;
    }

    public class CcAnomaliaCtaDerivadaSpDto
    {
        public string TipoDoc { get; set; } = string.Empty;
        public string NumDoc { get; set; } = string.Empty;
    }
}
