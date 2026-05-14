namespace Galileo_API.Models.ProGrX_Contabilidad
{
    public class CntXErEspecialDefinicionData
    {
        public int cod_er_especial { get; set; } = 0;
        public string descripcion { get; set; } = string.Empty;
        public string titulo { get; set; } = string.Empty;
        public string detalle { get; set; } = string.Empty;
    }

    public class CntXErEspecialCuentaNodeData
    {
        public string key { get; set; } = string.Empty;
        public string label { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string codigo { get; set; } = string.Empty;
        public bool @checked { get; set; } = false;
        public bool loaded { get; set; } = false;
        public List<CntXErEspecialCuentaNodeData> children { get; set; } = new();
    }

    public class CntXErEspecialCuentasGuardarRequest
    {
        public int cod_er_especial { get; set; } = 0;
        public string bloque { get; set; } = string.Empty;
        public string operacion { get; set; } = string.Empty;
        public List<string> cuentas { get; set; } = new();
    }

    public class CntXErEspecialArbolRequest
    {
        public int cod_er_especial { get; set; } = 0;
        public string bloque { get; set; } = string.Empty;
        public string operacion { get; set; } = string.Empty;
    }

    internal class CntXErEspecialCuentaData
    {
        public string cod_cuenta { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string cuenta_madre { get; set; } = string.Empty;
        public string tipo_cuenta { get; set; } = string.Empty;
    }

    internal class CntXErEspecialTipoCuentaData
    {
        public string tipo_cuenta { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    internal class CntXErEspecialCuentaMarcadaData
    {
        public string cod_cuenta { get; set; } = string.Empty;
    }
}