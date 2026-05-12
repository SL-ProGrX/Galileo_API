namespace Galileo_API.Models.ProGrX_Contabilidad
{
    public class CntXConPortalesConexionData
    {
        public string por_user { get; set; } = string.Empty;
        public string por_password { get; set; } = string.Empty;
        public string por_server { get; set; } = string.Empty;
        public string por_database { get; set; } = string.Empty;
    }

    public class CntXConPortalesDefinicionData : CntXConPortalesConexionData
    {
        public int cod_portal { get; set; } = 0;
        public string descripcion { get; set; } = string.Empty;
        public string observacion { get; set; } = string.Empty;
        public string registro_usuario { get; set; } = string.Empty;
        public string registro_fecha { get; set; } = string.Empty;
        public string actualiza_usuario { get; set; } = string.Empty;
        public string actualiza_fecha { get; set; } = string.Empty;
    }

    public class CntXConPortalesContabilidadData
    {
        public int cod_contabilidad { get; set; } = 0;
        public string nombre { get; set; } = string.Empty;
        public bool @checked { get; set; } = false;
    }

    public class CntXConPortalesConexionRequest : CntXConPortalesConexionData
    {
        public int cod_portal { get; set; } = 0;
    }

    public class CntXConPortalesGuardarRequest : CntXConPortalesConexionData
    {
        public int cod_portal { get; set; } = 0;
        public string descripcion { get; set; } = string.Empty;
        public string observacion { get; set; } = string.Empty;
        public List<int> contabilidades { get; set; } = new();
    }

    internal class CntXConPortalesDbData : CntXConPortalesConexionData
    {
        public int cod_portal { get; set; } = 0;
        public string descripcion { get; set; } = string.Empty;
        public string observacion { get; set; } = string.Empty;
        public string registro_usuario { get; set; } = string.Empty;
        public string registro_fecha { get; set; } = string.Empty;
        public string actualiza_usuario { get; set; } = string.Empty;
        public string actualiza_fecha { get; set; } = string.Empty;
    }

    internal class CntXConPortalesExternaData
    {
        public int cod_contabilidad { get; set; } = 0;
        public string nombre { get; set; } = string.Empty;
    }
}
