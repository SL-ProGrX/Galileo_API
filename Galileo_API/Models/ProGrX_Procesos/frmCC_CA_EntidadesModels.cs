namespace PgxAPI.Models.GEN
{
    public class PRM_CA_EntidadData
    {
        public string cod_entidad { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string numero_afiliado { get; set; } = string.Empty;
        public string formato { get; set; } = string.Empty;
        public string cod_cuenta { get; set; } = string.Empty;
        public bool activo { get; set; }
    }

    public class PRM_CA_EntidadUpsert
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
