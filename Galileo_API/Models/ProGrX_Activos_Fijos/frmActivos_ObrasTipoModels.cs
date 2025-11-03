using PgxAPI.Models.ProGrX.Bancos;

namespace PgxAPI.Models.ProGrX_Activos_Fijos
{
    public class Activos_ObrasTipoDesemDataLista
    {
        public int total { get; set; }
        public List<Activos_ObrasTipoDesemData> lista { get; set; } = new List<Activos_ObrasTipoDesemData>();
    }
    public class Activos_ObrasTipoDesemData
    {
        public string cod_desembolso { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool activo { get; set; }
        public bool isNew { get; set; } = false;
    }


    public class Activos_ObrasTipoDataLista
    {
        public int total { get; set; }
        public List<Activos_ObrasTipoData> lista { get; set; } = new List<Activos_ObrasTipoData>();
    }
    public class Activos_ObrasTipoData
    {
        public string cod_tipo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool activo { get; set; }
        public bool isNew { get; set; } = false;
    }



}
