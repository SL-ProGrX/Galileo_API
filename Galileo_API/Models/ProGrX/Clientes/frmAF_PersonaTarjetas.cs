namespace PgxAPI.Models.ProGrX.Clientes
{
    public class PersonaTarjetaDto
    {
        public string tarjeta_Numero { get; set; } = string.Empty;
        public string Tarjeta_Mask { get; set; } = string.Empty;
        public DateTime Tarjeta_Vence { get; set; }
        public string Tarjeta_Code { get; set; } = string.Empty;
        public string Tarjeta_Tipo { get; set; } = string.Empty;
    }

    public class PersonaTarjetaRegistroDto
    {
        public string Cedula { get; set; } = string.Empty;
        public string Tarjeta { get; set; } = string.Empty;
        public DateTime Vence { get; set; }
        public string Code { get; set; } = string.Empty;
        public string TipoMov { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public bool ValidaTarjeta { get; set; }
    }
}