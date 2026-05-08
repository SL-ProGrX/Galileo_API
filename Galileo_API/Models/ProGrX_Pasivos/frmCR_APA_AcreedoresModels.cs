namespace Galileo_API.Models.ProGrX_Pasivos
{
    public class FrmCrApaAcreedoresGridItem
    {
        public string cod_acreedor { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
    }

    public class FrmCrApaAcreedoresGridLista
    {
        public int total { get; set; } = 0;
        public List<FrmCrApaAcreedoresGridItem> lista { get; set; } = new();
    }

    public class FrmCrApaAcreedorDatosDto
    {
        public string cod_acreedor { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string telefono1 { get; set; } = string.Empty;
        public string telefono2 { get; set; } = string.Empty;
        public string direccion { get; set; } = string.Empty;
        public string website { get; set; } = string.Empty;
        public string cod_cuenta { get; set; } = string.Empty;
        public string cod_cuenta_desc { get; set; } = string.Empty;

        public string cod_cuenta_transitoria { get; set; } = string.Empty;
        public string cod_cuenta_transitoria_desc { get; set; } = string.Empty;

        public string cod_cuenta_gastos { get; set; } = string.Empty;
        public string cod_cuenta_gastos_desc { get; set; } = string.Empty;

        public string cod_cuenta_cargos { get; set; } = string.Empty;
        public string cod_cuenta_cargos_desc { get; set; } = string.Empty;

        public string cod_cuenta_comision { get; set; } = string.Empty;
        public string cod_cuenta_comision_desc { get; set; } = string.Empty;

        public int? banco_ck { get; set; }
        public string banco_ck_desc { get; set; } = string.Empty;

        public int? banco_dc { get; set; }
        public string banco_dc_desc { get; set; } = string.Empty;
    }

    public class FrmCrApaAcreedorGuardarRequest
    {
        public string cod_acreedor { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string telefono1 { get; set; } = string.Empty;
        public string telefono2 { get; set; } = string.Empty;
        public string direccion { get; set; } = string.Empty;
        public string website { get; set; } = string.Empty;
        public string cod_cuenta { get; set; } = string.Empty;
        public string cod_cuenta_transitoria { get; set; } = string.Empty;
        public string cod_cuenta_gastos { get; set; } = string.Empty;
        public string cod_cuenta_cargos { get; set; } = string.Empty;
        public string cod_cuenta_comision { get; set; } = string.Empty;
        public int? banco_ck { get; set; }
        public int? banco_dc { get; set; }
        public string usuario { get; set; } = string.Empty;
    }

    public class FrmCrApaBancoDto
    {
        public int item { get; set; }
        public string descripcion { get; set; } = string.Empty;
    }

}
