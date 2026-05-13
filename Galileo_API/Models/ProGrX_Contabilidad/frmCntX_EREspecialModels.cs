namespace Galileo_API.Models.ProGrX_Contabilidad
{
    public class CntXEREspecialDefinicionData
    {
        public int cod_er_especial { get; set; } = 0;
        public string descripcion { get; set; } = string.Empty;
        public string titulo { get; set; } = string.Empty;
        public string detalle { get; set; } = string.Empty;
    }

    public class CntXEREspecialCuentaNodeData
    {
        public string key { get; set; } = string.Empty;
        public string label { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string codigo { get; set; } = string.Empty;
        public bool @checked { get; set; } = false;
        public bool loaded { get; set; } = false;
        public List<CntXEREspecialCuentaNodeData> children { get; set; } = new();
    }

    public class CntXEREspecialCuentasGuardarRequest
    {
        public int cod_er_especial { get; set; } = 0;
        public string bloque { get; set; } = string.Empty;
        public string operacion { get; set; } = string.Empty;
        public List<string> cuentas { get; set; } = new();
    }

    public class CntXEREspecialArbolRequest
    {
        public int cod_er_especial { get; set; } = 0;
        public string bloque { get; set; } = string.Empty;
        public string operacion { get; set; } = string.Empty;
    }

    internal class CntXEREspecialCuentaData
    {
        public string cod_cuenta { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string cuenta_madre { get; set; } = string.Empty;
        public string tipo_cuenta { get; set; } = string.Empty;
    }

    internal class CntXEREspecialTipoCuentaData
    {
        public string tipo_cuenta { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    internal class CntXEREspecialCuentaMarcadaData
    {
        public string cod_cuenta { get; set; } = string.Empty;
    }
}