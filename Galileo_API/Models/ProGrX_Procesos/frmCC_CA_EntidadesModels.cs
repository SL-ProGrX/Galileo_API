namespace Galileo.Models.GEN
{

    public class CaEntidadLista
    {
        public int total { get; set; }
        public List<CaEntidadData> lista { get; set; } = new List<CaEntidadData>();
    }
    public class CaEntidadData
    {
        public string cod_entidad { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string numero_afiliado { get; set; } = string.Empty;
        public string formato { get; set; } = string.Empty;
        public string cod_cuenta { get; set; } = string.Empty;
        public string cod_cuenta_mask { get; set; } = string.Empty;
        public required bool activo { get; set; }
        public required bool isNew { get; set; }
    }

    public class PrmCaEntidadData
    {
        public string cod_entidad { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string numero_afiliado { get; set; } = string.Empty;
        public string formato { get; set; } = string.Empty;
        public string cod_cuenta { get; set; } = string.Empty;
        public bool activo { get; set; }
    }

    public class PrmCaEntidadUpsert
    {
        public string cod_entidad { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string numero_afiliado { get; set; } = string.Empty;
        public string formato { get; set; } = string.Empty;
        public string cod_cuenta { get; set; } = string.Empty;
        public bool activo { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
    }
}
