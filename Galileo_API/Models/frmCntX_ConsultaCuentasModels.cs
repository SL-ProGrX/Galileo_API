namespace PgxAPI.Models
{
    public class CtnxCuentasArbolModel
    {

        public object Data { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string ExpandedIcon { get; set; } = string.Empty;
        public string CollapsedIcon { get; set; } = string.Empty;
        public bool Expanded { get; set; }
        public bool Selectable { get; set; }
        public string StyleClass { get; set; } = string.Empty;
        public string Style { get; set; } = string.Empty;
        public List<CtnxCuentasArbolModel>? Children { get; set; }
        public bool leaf { get; set; }
    }

    public class CtnxCuentasDto
    {
        public string cod_cuenta { get; set; } = string.Empty;
        public string cod_cuenta_Mask { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool acepta_movimientos { get; set; }
        public string cuenta_madre { get; set; } = string.Empty;
        public string cod_divisa { get; set; } = string.Empty;
    }

    public class CuentaVarModel
    {
        public int Contabilidad { get; set; }
        public string Cuenta { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Divisa { get; set; } = string.Empty;
        public short Nivel { get; set; }
        public string cuentaMadre { get; set; } = string.Empty;
    }
}
