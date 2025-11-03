namespace PgxAPI.Models
{

    public class CapIndvDTO
    {


        public string Exc_Cap_Ind { get; set; } = string.Empty;
        public string Cedula { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public int Porcentaje { get; set; } = 0;
        public DateTime Vencimiento { get; set; }


    }
}