namespace Galileo.Models.ProGrX.Credito
{
    public class CRSeguimientoFirmasData
    {
        public string? tipo { get; set; }
        public string? cedula { get; set; }
        public string? nombre { get; set; }
        public required bool firma { get; set; }
        public required int operacion { get; set; }
    }
}