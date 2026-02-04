namespace Galileo_API.Models.ProGrX_Contabilidad
{
    public class ArfUnidadesData
    {
        public string cod_local { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string cod_unidad { get; set; } = string.Empty;
        public string cod_centro_costo { get; set; } = string.Empty;
        public bool activo { get; set; } = false;
        public string contacto_nombre { get; set; } = string.Empty;
        public string email_01 { get; set; } = string.Empty;
        public string email_02 { get; set; } = string.Empty;
        public string telefono_01 { get; set; } = string.Empty;
        public string telefono_02 { get; set; } = string.Empty;
        public string website { get; set; } = string.Empty;
        public string provincia { get; set; } = string.Empty;
        public string apto_postal { get; set; } = string.Empty;
        public string canton { get; set; } = string.Empty;
        public string distrito { get; set; } = string.Empty;
        public string direccion { get; set; } = string.Empty;
        public DateTime registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime? modifica_fecha { get; set; }
        public string? modifica_usuario { get; set; }
        public string provdesc { get; set; } = string.Empty;
        public string cantondesc { get; set; } = string.Empty;
        public string distdesc { get; set; } = string.Empty;
    }

}
