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
}
