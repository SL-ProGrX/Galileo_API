using Galileo.Models;

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

    public class FrmCrApaContactoGridDto
    {
        public string codigo { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string tel_celular { get; set; } = string.Empty;
        public string tel_trabajo { get; set; } = string.Empty;
        public string fax { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
    }

    public class FrmCrApaContactosListaDto
    {
        public int total { get; set; } = 0;
        public List<FrmCrApaContactoGridDto> lista { get; set; } = new();
    }

    public class FrmCrApaContactoGuardarRequest
    {
        public string cod_acreedor { get; set; } = string.Empty;
        public string codigo { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string tel_celular { get; set; } = string.Empty;
        public string tel_trabajo { get; set; } = string.Empty;
        public string fax { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public bool isNew { get; set; } = false;
    }

    public class FrmCrApaAutorizadoGridDto
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
    }

    public class FrmCrApaAutorizadosListaDto
    {
        public int total { get; set; } = 0;
        public List<FrmCrApaAutorizadoGridDto> lista { get; set; } = new();
    }

    public class FrmCrApaAutorizadoGuardarRequest
    {
        public string cod_acreedor { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public bool isNew { get; set; } = false;
    }

    public static class CrApaAcreedoresVariables
    {
        public const string vCodAcreedor = "El código del acreedor es requerido.";
    }

    public class CrApaAcreedoresListaParameters
    {
        public int codEmpresa { get; set; } = 0;
        public string? cod_acreedor { get; set; }
        public FiltrosLazyLoadData? filtros { get; set; }
        public string? defaultSortField { get; set; }
        public Func<string, int>? resolveSortCode { get; set; }
        public string? sqlCount { get; set; }
        public string? sqlData { get; set; }
    }

    public class CrApaAcreedorDatosParameters
    {
        public int codEmpresa { get; set; } = 0;
        public string codAcreedor { get; set; } = string.Empty;
        public string identificador { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public bool isNew { get; set; } = false;
        public object parametros { get; set; } = new();
        public string mensajeIdentificadorRequerido { get; set; } = string.Empty;
        public string mensajeNombreRequerido { get; set; } = string.Empty;
        public string mensajeDuplicado { get; set; } = string.Empty;
        public string mensajeNoEncontrado { get; set; } = string.Empty;
        public string sqlExiste { get; set; } = string.Empty;
        public string sqlInsert { get; set; } = string.Empty;
        public string sqlUpdate { get; set; } = string.Empty;
        public object parametrosExiste { get; set; } = new();
    }

}
