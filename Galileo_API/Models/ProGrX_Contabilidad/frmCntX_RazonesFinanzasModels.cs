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
}
