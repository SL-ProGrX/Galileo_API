namespace PgxAPI.Models
{
    public class MovimientoBE
    {
        public int Modulo { get; set; }
        public string Movimiento { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Registro_Usuario { get; set; } = string.Empty;
        public DateTime Registro_Fecha { get; set; }
    }

    public class ErrorMovimientoBEDTO
    {
        public int Code { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
