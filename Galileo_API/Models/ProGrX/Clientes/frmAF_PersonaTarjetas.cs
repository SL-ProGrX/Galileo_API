namespace Galileo.Models.ProGrX.Clientes
{
    public class PersonaTarjetaDto
    {
        public string tarjeta_Numero { get; set; } = string.Empty;
        public string tarjeta_Mask { get; set; } = string.Empty;
        public DateTime? tarjeta_Vence { get; set; }
        public string tarjeta_Code { get; set; } = string.Empty;
        public string tarjeta_Tipo { get; set; } = string.Empty;
        public string dia_apl_ca { get; set; } = string.Empty;
    }

    public class PersonaTarjetaRegistroDto
    {
        public string cedula { get; set; } = string.Empty;
        public string tarjeta { get; set; } = string.Empty;
        public DateTime? vence { get; set; }
        public string code { get; set; } = string.Empty;
        public string tipoMov { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public bool validaTarjeta { get; set; } = false;
        public string dia_apl_ca { get; set; } = "1";
    }
}