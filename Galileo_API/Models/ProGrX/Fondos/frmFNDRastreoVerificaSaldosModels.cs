namespace Galileo.Models.ProGrX.Fondos
{
    public class FndVerificacionSaldoDto
    {
        public int cod_operadora { get; set; }
        public required string cod_plan { get; set; }
        public int cod_contrato { get; set; }
        public decimal saldo_inicial { get; set; }
        public decimal debitos { get; set; }
        public decimal creditos { get; set; }
        public decimal sf_calculado { get; set; }
        public decimal saldo_final { get; set; }
        public decimal diferencia { get; set; }
        public required string identificacion { get; set; }
        public required string nombre { get; set; }
    }
}