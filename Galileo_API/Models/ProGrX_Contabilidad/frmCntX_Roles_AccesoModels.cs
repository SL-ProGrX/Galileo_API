namespace Galileo_API.Models.ProGrX_Contabilidad
{
    public class CntXAcRolDto
    {
        public int Cod_Contabilidad { get; set; }
        public string Cod_Rol { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public short Control { get; set; } 
        public short Activo { get; set; }
        public string Registro_Fecha { get; set; } = string.Empty;
        public string Registro_Usuario { get; set; } = string.Empty;
    }

    public class CntXAcCuentaDto
    {
        public string Cod_Cuenta { get; set; } = string.Empty;
        public string Cod_Cuenta_Mask { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Cod_Divisa { get; set; } = string.Empty;
        public short Acepta_Movimientos { get; set; }
    }

    public class CntXAcUnidadDto
    {
        public int Cod_Contabilidad { get; set; }
        public string Cod_Unidad { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public short Asignado { get; set; }
    }

    public class CntXAcCentroCostoDto
    {
        public int Cod_Contabilidad { get; set; }
        public string Cod_Unidad { get; set; } = string.Empty;
        public string Cod_Centro_Costo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public short Asignado { get; set; }
    }

    public class CntXAcMiembroDto
    {
        public int Cod_Contabilidad { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public string Usuario_Nombre { get; set; } = string.Empty;
        public short Asignado { get; set; }
    }

    public class CntXAcCuentaAsignaParams
    {
        public required int CodContabilidad { get; set; }
        public string Rol { get; set; } = string.Empty;
        public string Cuenta { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public char Mov { get; set; } = 'A'; // 'A' (asignar) o 'E' (eliminar)
    }

    public class CntXAcUnidadAsignaParams
    {
        public required int CodContabilidad { get; set; }
        public string Rol { get; set; } = string.Empty;
        public string Unidad { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public char Mov { get; set; } = 'A'; // 'A' (asignar) o 'E' (eliminar)
    }

    public class CntXAcCentroCostoAsignaParams
    {
        public required int CodContabilidad { get; set; }
        public string Rol { get; set; } = string.Empty;
        public string Unidad { get; set; } = string.Empty;
        public string CentroCosto { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public char Mov { get; set; } = 'A'; // 'A' (asignar) o 'E' (eliminar)
    }

    public class CntXAcMiembroAsignaParams
    {
        public required int CodContabilidad { get; set; }
        public string Rol { get; set; } = string.Empty;
        public string Miembro { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public char Mov { get; set; } = 'A'; // 'A' (asignar) o 'E' (eliminar)
    }

    public class CntXAcRolAddParams
    {
        public required int Codigo { get; set; }
        public string Rol { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public short Control { get; set; }
        public short Activo { get; set; }
        public string Usuario { get; set; } = string.Empty;
    }

    public class CntXAcRolDeleteParams
    {
        public required int Codigo { get; set; }
        public string Rol { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
    }
}
