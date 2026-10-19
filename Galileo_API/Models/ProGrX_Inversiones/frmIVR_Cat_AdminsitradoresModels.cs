namespace Galileo.Models.ProGrX_Inversiones
{
    public sealed class IvrCatAdministradorInicialData
    {
        public List<DropDownListaGenericaModel<int>> bancos
        {
            get;
            set;
        } = [];

        public List<DropDownListaGenericaModel<string>>
            tipos_administrador
        {
            get;
            set;
        } = [];

        public List<DropDownListaGenericaModel<string>> estados
        {
            get;
            set;
        } = [];

        public List<DropDownListaGenericaModel<string>>
            tipos_documento
        {
            get;
            set;
        } = [];
    }

    public sealed class IvrCatAdministradorData
    {
        public string cod_administrador { get; set; } =
            string.Empty;

        public string descripcion { get; set; } =
            string.Empty;

        public string identificacion { get; set; } =
            string.Empty;

        public string tipo_administrador { get; set; } =
            string.Empty;

        public string tipo_desc { get; set; } =
            string.Empty;

        public string estado { get; set; } =
            string.Empty;

        public string direccion { get; set; } =
            string.Empty;

        public string apto_postal { get; set; } =
            string.Empty;

        public string website { get; set; } =
            string.Empty;

        public string email_01 { get; set; } =
            string.Empty;

        public string email_02 { get; set; } =
            string.Empty;

        public string telefono1 { get; set; } =
            string.Empty;

        public string telefono2 { get; set; } =
            string.Empty;

        public string fax { get; set; } =
            string.Empty;

        public string cod_cuenta_transito { get; set; } =
            string.Empty;

        public string cod_cuenta_transito_mask
        {
            get;
            set;
        } = string.Empty;

        public string cod_cuenta_transito_desc
        {
            get;
            set;
        } = string.Empty;

        public string tes_emitir { get; set; } =
            string.Empty;

        public int tes_banco { get; set; }

        public string banco_desc { get; set; } =
            string.Empty;
    }

    public sealed class IvrCatAdministradorBusquedaData
    {
        public string cod_administrador { get; set; } =
            string.Empty;

        public string identificacion { get; set; } =
            string.Empty;

        public string descripcion { get; set; } =
            string.Empty;
    }

    public sealed class IvrCatAdministradorNavegacionData
    {
        public string cod_administrador { get; set; } =
            string.Empty;
    }

    public sealed class IvrCatAdministradorGuardarRequest
    {
        public string cod_administrador { get; set; } =
            string.Empty;

        public string tipo_administrador { get; set; } =
            string.Empty;

        public string identificacion { get; set; } =
            string.Empty;

        public string descripcion { get; set; } =
            string.Empty;

        public string estado { get; set; } =
            string.Empty;

        public string telefono1 { get; set; } =
            string.Empty;

        public string telefono2 { get; set; } =
            string.Empty;

        public string fax { get; set; } =
            string.Empty;

        public string email_01 { get; set; } =
            string.Empty;

        public string email_02 { get; set; } =
            string.Empty;

        public string apto_postal { get; set; } =
            string.Empty;

        public string website { get; set; } =
            string.Empty;

        public string direccion { get; set; } =
            string.Empty;

        public string cod_cuenta_transito { get; set; } =
            string.Empty;

        public string tes_emitir { get; set; } =
            string.Empty;

        public int tes_banco { get; set; }

        public string usuario { get; set; } =
            string.Empty;
    }

    public sealed class IvrCatAdministradorCuentaContableData
    {
        public bool es_valida { get; set; }

        public string cod_cuenta { get; set; } =
            string.Empty;

        public string cod_cuenta_mask { get; set; } =
            string.Empty;

        public string descripcion { get; set; } =
            string.Empty;
    }

    public sealed class IvrCatAdministradorCuentaBancariaData
    {
        public string cuenta_interna { get; set; } =
            string.Empty;

        public string banco { get; set; } =
            string.Empty;

        public string tipo_desc { get; set; } =
            string.Empty;

        public string cod_divisa { get; set; } =
            string.Empty;

        public bool cuenta_interbanca { get; set; }

        public string cuenta_interbanca_desc
        {
            get;
            set;
        } = string.Empty;

        public string destino { get; set; } =
            string.Empty;

        public bool activa { get; set; }

        public string estado_desc { get; set; } =
            string.Empty;

        public DateTime? registro_fecha { get; set; }

        public string registro_usuario { get; set; } =
            string.Empty;
    }

    public sealed class IvrCatAdministradorPortafolioData
    {
        public string cod_portafolio { get; set; } =
            string.Empty;

        public string descripcion { get; set; } =
            string.Empty;

        public bool asignado { get; set; }
    }

    public sealed class
        IvrCatAdministradorPortafolioActualizarRequest
    {
        public string cod_portafolio { get; set; } =
            string.Empty;

        public string cod_administrador { get; set; } =
            string.Empty;

        public bool asignado { get; set; }

        public string usuario { get; set; } =
            string.Empty;
    }

    public sealed class IvrCatAdministradorInstrumentoData
    {
        public string cod_instrumento { get; set; } =
            string.Empty;

        public string descripcion { get; set; } =
            string.Empty;

        public bool asignado { get; set; }
    }

    public sealed class
        IvrCatAdministradorInstrumentoActualizarRequest
    {
        public string cod_administrador { get; set; } =
            string.Empty;

        public string cod_instrumento { get; set; } =
            string.Empty;

        public bool asignado { get; set; }

        public string usuario { get; set; } =
            string.Empty;
    }

    public sealed class IvrCatAdministradorContactoData
    {
        public long cod_contacto { get; set; }

        public string nombre { get; set; } =
            string.Empty;

        public string celular { get; set; } =
            string.Empty;

        public string telefono { get; set; } =
            string.Empty;

        public string email_01 { get; set; } =
            string.Empty;

        public string email_02 { get; set; } =
            string.Empty;
    }
}