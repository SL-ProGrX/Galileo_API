namespace Galileo.Models.ProGrX.Bancos
{
    public class TesImpresorasDto
    {
        public int cod_impresora_cheque { get; set; } = 0;
        public int cod_impresora_recibo { get; set; } = 0;
        public string descripcion_cheque { get; set; } = string.Empty;
        public string descripcion_recibo { get; set; } = string.Empty;
        public string registro_usuario { get; set; } = string.Empty;
    }
}