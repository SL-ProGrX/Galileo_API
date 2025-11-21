namespace Galileo.Models
{
    public class BitacoraBeneInsertarDto
    {
        public int EmpresaId { get; set; }
        public int id_bitacora { get; set; }
        public string cod_beneficio { get; set; } = string.Empty;
        public long? consec { get; set; }
        public string movimiento { get; set; } = string.Empty;
        public string detalle { get; set; } = string.Empty;
        public string registro_usuario { get; set; } = string.Empty;
    }
}
