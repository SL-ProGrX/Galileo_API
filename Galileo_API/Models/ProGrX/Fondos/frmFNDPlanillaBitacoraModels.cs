namespace Galileo.Models.ProGrX.Fondos
{
    public class FndPrmBitacoraDto
    {
        public int id_seq { get; set; }
        public string? proceso { get; set; }
        public int cod_institucion { get; set; }
        public string? cod_plan { get; set; }
        public string? gestion { get; set; }
        public string? transaccion { get; set; }
        public string? usuario { get; set; }
        public DateTime fecha { get; set; }
        public string? documento { get; set; }
        public int casos { get; set; }
        public decimal monto { get; set; }
    }
}