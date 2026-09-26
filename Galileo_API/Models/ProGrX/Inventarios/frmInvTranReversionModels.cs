namespace Galileo.Models.INV
{
    public class TranReversionInsert
    {
        public string boleta { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string cod_entsal { get; set; } = string.Empty;
        public DateTime? fecha { get; set; }
        public string notas { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
    }

    public class InvProducReversion : InvProducLineas
    {
        public string bodega_d { get; set; } = string.Empty;
    }
}