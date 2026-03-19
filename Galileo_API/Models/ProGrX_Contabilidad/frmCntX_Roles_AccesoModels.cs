namespace Galileo_API.Models.ProGrX_Contabilidad
{
    public class CntXAcRolDto
    {
        public int Cod_Contabilidad { get; set; }
        public string Cod_Rol { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Control { get; set; } = string.Empty;
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
}
