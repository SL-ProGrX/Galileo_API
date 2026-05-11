namespace Galileo_API.Models.ProGrX_Contabilidad
{
    public class CntXConsolidacionDefinicionData
    {
        public int cod_consolida { get; set; } = 0;
        public string descripcion { get; set; } = string.Empty;
        public int cod_contabilidad { get; set; } = 0;
        public string nombre_contabilidad { get; set; } = string.Empty;
        public int nivel { get; set; } = 0;
        public string registro_usuario { get; set; } = string.Empty;
        public string registro_fecha { get; set; } = string.Empty;
        public string actualiza_usuario { get; set; } = string.Empty;
        public string actualiza_fecha { get; set; } = string.Empty;
    }

    public class CntXConsolidacionContabilidadData
    {
        public int cod_contabilidad { get; set; } = 0;
        public string nombre { get; set; } = string.Empty;
        public bool @checked { get; set; } = false;
    }

    public class CntXConsolidacionPortalContaData
    {
        public int cod_portal { get; set; } = 0;
        public int cod_contabilidad { get; set; } = 0;
    }

    public class CntXConsolidacionPortalNodeData
    {
        public string key { get; set; } = string.Empty;
        public string label { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public int? cod_portal { get; set; }
        public int? cod_contabilidad { get; set; }
        public bool @checked { get; set; } = false;
        public bool loaded { get; set; } = false;
        public List<CntXConsolidacionPortalNodeData> children { get; set; } = new();
    }

    public class CntXConsolidacionesGuardarRequest
    {
        public int cod_consolida { get; set; } = 0;
        public string descripcion { get; set; } = string.Empty;
        public int cod_contabilidad { get; set; } = 0;
        public int nivel { get; set; } = 0;
        public List<int> contabilidades_locales { get; set; } = new();
        public List<CntXConsolidacionPortalContaData> contabilidades_portales { get; set; } = new();
    }

    internal class CntXPortalData
    {
        public int cod_portal { get; set; } = 0;
        public string descripcion { get; set; } = string.Empty;
        public string por_user { get; set; } = string.Empty;
        public string por_password { get; set; } = string.Empty;
        public string por_server { get; set; } = string.Empty;
        public string por_database { get; set; } = string.Empty;
        public int cod_contabilidad { get; set; } = 0;
    }

    internal class CntXPortalContaRelacionData
    {
        public int cod_portal { get; set; } = 0;
        public int cod_contabilidad { get; set; } = 0;
    }

    internal class CntXMascaraContaData
    {
        public int nivel1 { get; set; } = 0;
        public int nivel2 { get; set; } = 0;
        public int nivel3 { get; set; } = 0;
        public int nivel4 { get; set; } = 0;
        public int nivel5 { get; set; } = 0;
    }

    internal class CntXContaNombreData
    {
        public int cod_contabilidad { get; set; } = 0;
        public string nombre { get; set; } = string.Empty;
    }
}
