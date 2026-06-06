namespace Galileo.Models.ProGrX.Clientes
{
    public class AfPadronData
    {
        public string? identificacion { get; set; }
        public string? id_alterna { get; set; }
        public string? nombre { get; set; }
        public DateTime? fecha_ingreso { get; set; }
    }

    public class AfSalarioData
    {
        public string? identificacion { get; set; }
        public string? divisa { get; set; }
        public DateTime? fecha { get; set; }
        public required decimal salario_bruto { get; set; }
        public required decimal rebajos { get; set; }
        public required decimal salario_neto { get; set; }
        public string? embargos { get; set; }
    }
}