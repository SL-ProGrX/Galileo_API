namespace PgxAPI.Models
{
    public class SYS_Cuentas_BancariasDTO
    {
        public string Identificacion { get; set; } = string.Empty;
        public string Cod_Banco { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string Cod_Divisa { get; set; } = string.Empty;
        public string Modulo { get; set; } = string.Empty;
        public string Destino { get; set; } = string.Empty;
        public string Cuenta_Interna { get; set; } = string.Empty;
        public bool Cuenta_Interbanca { get; set; } = false;
        public bool Cuenta_Default { get; set; } = false;
        public bool Activa { get; set; } = false;
        public string Registro_Fecha { get; set; } = string.Empty;
        public string Registro_Usuario { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Tipo_Desc { get; set; } = string.Empty;
        public string DataKey { get; set; } = string.Empty;
    }

    public class BancosCC
    {
        public string Cod_Grupo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;

    }

    public class ValidacionCC
    {
        public string LCTA_Interna { get; set; } = string.Empty;
        public string LCTA_InterBancaria { get; set; } = string.Empty;
    }

    public class SYS_Cuentas_Bancarias_List
    {
        public int id_banco { get; set; }
        public string descripcion { get; set; }
        public string cta { get; set; }
        public int idx { get; set; }
        public string cod_divisa { get; set; }
        public string entidad_desc { get; set; }
    }

    public class SIF_FormasPagoBancoAsgDTO
    {
        public int IdBanco { get; set; }
        public string CodFormaPago { get; set; }
        public string RegistroUsuario { get; set; }
    }


}
