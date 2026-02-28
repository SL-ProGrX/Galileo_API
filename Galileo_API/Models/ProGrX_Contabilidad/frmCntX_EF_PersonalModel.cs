namespace Galileo_API.Models.ProGrX_Contabilidad
{
    public class CntXEfPersonalDto
    {
        public string Cod_Ef { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public short Activo { get; set; }
    }

    public class CntXEfPersonalSaveParams
    {
        public required int CodContabilidad { get; set; }
        public string CodEf { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public short? Activo { get; set; }
        public string RegistroUsuario { get; set; } = string.Empty;
    }

    public class CntXEfPersonalDeleteParams
    {
        public required int CodContabilidad { get; set; }
        public string CodEf { get; set; } = string.Empty;
    }

    public class CntXEfSeccionDto
    {
        public string? ItemId { get; set; }
        public string? ItemIdMadre { get; set; }
        public string? Prioridad { get; set; }
        public int EsTitulo { get; set; }
        public int Totales { get; set; }
        public string Descripcion { get; set; } = string.Empty;
    }

    public class CntXEfSeccionSaveParams
    {
        public string CodEf { get; set; } = string.Empty;
        public int CodContabilidad { get; set; }
        public string ItemId { get; set; } = string.Empty;
        public string? ItemIdMadre { get; set; }
        public int Prioridad { get; set; }
        public int EsTitulo { get; set; }
        public int Totales { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public string RegistroUsuario { get; set; } = string.Empty;
    }

    public class CntXEfSeccionDeleteParams
    {
        public string CodEf { get; set; } = string.Empty;
        public int CodContabilidad { get; set; }
        public string ItemId { get; set; } = string.Empty;
        public string RegistroUsuario { get; set; } = string.Empty;
    }

    public class CntXEfSeccionSimpleDto
    {
        public string ItemId { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }

    public class CntXCuentaDto
    {
        public string CodCuenta { get; set; } = string.Empty;
        public string CodCuentaMask { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string CodDivisa { get; set; } = string.Empty;
        public int AceptaMovimientos { get; set; }
    }

    public class CntXCuentaFiltroParams
    {
        public int CodContabilidad { get; set; }
        public string CodEf { get; set; } = string.Empty;
        public string ItemId { get; set; } = string.Empty;
        public string? CuentaInicio { get; set; }
        public string? CuentaFin { get; set; }
    }

    public class CntXCuentaAsignadaDto : CntXCuentaDto
    {
        public int Asignado { get; set; }
    }

    public class CntXFxAsignadaDto
    {
        public string CodFx { get; set; } = string.Empty;
        public string FxName { get; set; } = string.Empty;
        public int Asignado { get; set; }
    }

    public class CntEfBase
    {
        public int CodContabilidad { get; set; }
        public string CodEf { get; set; } = string.Empty;
        public string RegistroUsuario { get; set; } = string.Empty;
    }

    public class CntXEfCuentaProcParams : CntEfBase
    {
        public string ItemId { get; set; } = string.Empty;
        public string Cuenta { get; set; } = string.Empty;
        public char Movimiento { get; set; } // 'A' (agregar) o 'E' (eliminar)
    } 

    public class CntXEfFxProcParams : CntEfBase
    {
        public string ItemId { get; set; } = string.Empty;
        public string CodFx { get; set; } = string.Empty;
        public char Movimiento { get; set; } // 'A' (agregar) o 'E' (eliminar)
    }

    public class CntXEfProcesaParams : CntEfBase
    {
        public int Anio { get; set; }
        public int Mes { get; set; }
        public char Tipo { get; set; } = 'A'; // 'A' (anual) o 'T' (trimestre)
        public int Expresado { get; set; } = 1;
    }
}
