namespace Galileo.Models.ProGrX.Clientes
{
    public class FndTrasladoSocioSimple
    {
        public required string Cedula { get; set; }
        public required string Nombre { get; set; }
    }

    public class FndTrasladoContratoDisponible
    {
        public required string Cod_Contrato { get; set; }
        public required string Cod_Plan { get; set; }
        public decimal Disponible { get; set; }
        public required string Descripcion { get; set; }
    }

    public class FndTrasladoFondosRequest
    {
        public required string PlanOrigen { get; set; }
        public required int ContratoOrigen { get; set; }
        public required string Cedula { get; set; }
        public required decimal Monto { get; set; }
        public required string PlanDestino { get; set; }
        public required int ContratoDestino { get; set; }
        public required string Usuario { get; set; }
        public string? Nota { get; set; }
        public string App { get; set; } = "ProGrX";
    }

    public class FndTrasladoFondosResult
    {
        public required string TipoDoc { get; set; }
        public required string NumDoc { get; set; }
    }
}