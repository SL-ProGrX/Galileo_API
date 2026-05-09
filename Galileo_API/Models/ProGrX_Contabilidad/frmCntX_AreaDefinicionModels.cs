namespace Galileo_API.Models.ProGrX_Contabilidad
{
    public class AreaDefinicionDto
    {
        public int Cod_Area { get; set; }
        public string Descripcion { get; set; } = string.Empty;
    }

    public class TipoCuentaDto
    {
        public string Tipo_Cuenta { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }

    public class CuentaNodoDto
    {
        public string Cod_Cuenta { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Acepta_Movimientos { get; set; } = string.Empty;
    }

    public class ExisteDto
    {
        public int Existe { get; set; }
    }

    public class AreaCuentaDetalleDto
    {
        public string Cod_Cuenta { get; set; } = string.Empty;
        public string Acepta_Movimientos { get; set; } = string.Empty;
        public string Cuenta_Madre { get; set; } = string.Empty;
        public int Nivel { get; set; }
    }

    public class UnidadDto
    {
        public string Cod_Unidad { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }

    public class CentroCostoDto
    {
        public string Cod_Centro_Costo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }
}
