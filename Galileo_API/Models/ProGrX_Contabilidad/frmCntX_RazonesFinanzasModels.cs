namespace Galileo_API.Models.ProGrX_Contabilidad
{
    public class CntXRazonesFinanzasDto
    {
        public string CodGrupo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public short Activa { get; set; }
    }

    public class CntXRazonesFinanzasSaveParams
    {
        public required int CodContabilidad { get; set; }
        public string CodGrupo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public short? Activa { get; set; }
        public string RegistroUsuario { get; set; } = string.Empty;
    }

    public class CntXRazonFinancieraDto
    {
        public string CodRazon { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Resultado { get; set; } = string.Empty;
        public string Grupo { get; set; } = string.Empty;
    }

    public class CntXRazonFinancieraSaveParams
    {
        public required int CodContabilidad { get; set; }
        public string CodRazon { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Resultado { get; set; } = string.Empty;
        public string CodGrupo { get; set; } = string.Empty;
        public string RegistroUsuario { get; set; } = string.Empty;
    }

    public class CntXRazonFinancieraTipoDto
    {
        public string CodGrupo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }

    public class CntXRazonNotasDto
    {
        public string Notas { get; set; } = string.Empty;
        public string Formula { get; set; } = string.Empty;
    }

    public class CntXRazonDetalleDto
    {
        public required int Idx { get; set; }
        public string CodRazon { get; set; } = string.Empty;
        public required int CodContabilidad { get; set; }
        public string CodCuenta { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Cod_Cuenta_Mask { get; set; } = string.Empty;
        public string Operador { get; set; } = string.Empty;
    }

    public class CntXRazonDetalleIdxDto
    {
        public int Idx { get; set; }
    }

    public class CntXRazonNotasUpdateParams : CntXRazonNotasDto
    {
        public required int CodContabilidad { get; set; }
        public string CodGrupo { get; set; } = string.Empty;
        public string CodRazon { get; set; } = string.Empty;
    }

    public class CntXRazonesReporteInsertParams
    {
        public string Usuario { get; set; } = string.Empty;
        public required int CodContabilidad { get; set; }
        public string CodRazon { get; set; } = string.Empty;
        public decimal Monto { get; set; }
    }

    public class CntXRazonesReporteUpdateParams
    {
        public string Usuario { get; set; } = string.Empty;
        public required int CodContabilidad { get; set; }
        public string CodRazon { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public string Mes { get; set; } = "Mes01"; // Mes01, Mes02, etc.
    }

    public class CntXRazonFormulaDto
    {
        public string Formula { get; set; } = string.Empty;
    }

    public class CntXRazonMontoParams
    {
        public required int CodContabilidad { get; set; }
        public string CodRazon { get; set; } = string.Empty;
        public int Idx { get; set; }
        public int Anio { get; set; }
        public int Mes { get; set; }
        public string Unidad { get; set; } = "TODOS";
    }
    public class CntXRazonMontoDto
    {
        public decimal Monto { get; set; }
    }
}
